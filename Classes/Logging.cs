using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Classes
{
    public static class Logging
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        private static string GetClientIp()
        {
            return _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "IP alınamadı";
        }
        public static async Task LogAdd(string type, string message)
        {
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@LogType", type),
                new SqlParameter("@ErrorMessage", message),
                new SqlParameter("@IPAdress", GetClientIp())
            };
            await SQLCrud.InsertUpdateDeleteAsync("AdminLogsAdd", parameters, CommandType.StoredProcedure);
        }
    }
}