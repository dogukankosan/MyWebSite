using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Classes
{
    public static class SQLCrud
    {
        private static IConfiguration _configuration;
        private static IHttpContextAccessor _httpContextAccessor;
        public static void Configure(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }
        private static string ConnectionString => _configuration.GetConnectionString("DefaultConnection");
        private static string GetClientIp()
        {
            return _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "IP alınamadı";
        }
        private static bool ContainsDangerousSql(string query)
        {
            string[] blacklist = {
                "char(", "nchar(", "varchar(", "cursor ", "declare ",
                "drop ", "exec(", "execute ", "fetch ", "kill ",
                "sys.", "sysobjects", "syscolumns", "union ", "information_schema"
            };
            return blacklist.Any(dangerous =>
                query.IndexOf(dangerous, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        public static async Task<bool> InsertUpdateDeleteAsync(string procedureName, List<SqlParameter> parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (ContainsDangerousSql(procedureName))
            {
                await Logging.LogAdd("[SQL Injection Engellendi]", $"Zararlı içerik bulundu: {procedureName}");
                return false;
            }
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                using SqlCommand cmd = new(procedureName, conn)
                {
                    CommandType = commandType,
                    CommandTimeout = 120
                };
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters.ToArray());
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("[HATA - InsertUpdateDeleteAsync]", ex.ToString());
                return false;
            }
        }
        public static async Task<T> ExecuteScalarAsync<T>(string procedureName, List<SqlParameter> parameters = null, T fallbackValue = default, CommandType commandType = CommandType.StoredProcedure)
        {
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                using SqlCommand cmd = new(procedureName, conn)
                {
                    CommandType = commandType
                };
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters.ToArray());
                object result = await cmd.ExecuteScalarAsync();
                return (result == null || result == DBNull.Value) ? fallbackValue : (T)Convert.ChangeType(result, typeof(T));
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("[HATA - ExecuteScalarAsync]", ex.ToString());
                return fallbackValue;
            }
        }
        public static async Task<List<T>> ExecuteModelListAsync<T>(
            string procedureName,
            List<SqlParameter> parameters,
            Func<SqlDataReader, T> mapper,
            CommandType commandType = CommandType.StoredProcedure)
        {
            List<T> list = new();
            using SqlConnection conn = new(ConnectionString);
            using SqlCommand cmd = new(procedureName, conn)
            {
                CommandType = commandType
            };
            if (parameters != null)
                cmd.Parameters.AddRange(parameters.ToArray());
            await conn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(mapper(reader));
            }
            return list;
        }
    }
}