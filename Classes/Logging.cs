using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Classes
{
    public static class Logging
    {
        public static async Task LogAdd(string type, string message, string connection, string ipAdress)
        {
            using (SqlConnection con = new(connection))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminLogsAdd", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LogType", type);
                    cmd.Parameters.AddWithValue("@ErrorMessage", message);
                    cmd.Parameters.AddWithValue("@IPAdress", ipAdress);
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}