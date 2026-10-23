using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace MyWebSite.Classes
{
    // Log yazma asla hata fırlatmaz ve SQLCrud'u kullanmaz.
    // Eskiden SQLCrud hata → LogAdd → SQLCrud hata → LogAdd ... şeklinde sonsuz döngüye giriyordu.
    // DB'ye yazılamazsa log, uygulama klasöründeki logs/ altına dosya olarak yazılır.
    public static class Logging
    {
        private const int MaxMessageLength = 4000;
        private const int MaxTypeLength = 200;
        private const long MaxLogFileBytes = 10 * 1024 * 1024; // günlük dosya en fazla 10 MB

        private static readonly string LogFolder = Path.Combine(AppContext.BaseDirectory, "logs");
        private static readonly object FileLock = new();

        private static IHttpContextAccessor? _httpContextAccessor;
        private static string? _connectionString;

        public static void Configure(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _httpContextAccessor = httpContextAccessor;
        }

        private static string GetClientIp()
        {
            try
            {
                return _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "IP alınamadı";
            }
            catch
            {
                return "IP alınamadı";
            }
        }

        private static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value[..maxLength];
        }

        public static async Task LogAdd(string type, string message)
        {
            string safeType = Truncate(type, MaxTypeLength);
            string safeMessage = Truncate(message, MaxMessageLength);
            try
            {
                using SqlConnection conn = new(_connectionString);
                await conn.OpenAsync();
                using SqlCommand cmd = new("AdminLogsAdd", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 15
                };
                cmd.Parameters.AddWithValue("@LogType", safeType);
                cmd.Parameters.AddWithValue("@ErrorMessage", safeMessage);
                cmd.Parameters.AddWithValue("@IPAdress", GetClientIp());
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                WriteToFile(safeType, safeMessage, ex);
            }
        }

        // DB'ye ihtiyaç duymadan dosyaya yazar. Çökme anında da kullanılır.
        public static void WriteToFile(string type, string message, Exception? logError = null)
        {
            try
            {
                Directory.CreateDirectory(LogFolder);
                string filePath = Path.Combine(LogFolder, $"log-{DateTime.Now:yyyy-MM-dd}.txt");

                StringBuilder sb = new();
                sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {type}");
                sb.AppendLine(message);
                if (logError != null)
                    sb.AppendLine($"(DB'ye log yazılamadı: {logError.Message})");
                sb.AppendLine(new string('-', 60));

                lock (FileLock)
                {
                    FileInfo info = new(filePath);
                    if (info.Exists && info.Length > MaxLogFileBytes) return; // diski doldurmasın
                    File.AppendAllText(filePath, sb.ToString(), Encoding.UTF8);
                }
            }
            catch
            {
                // Dosyaya da yazılamıyorsa yapacak bir şey yok; uygulamayı düşürme.
            }
        }
    }
}
