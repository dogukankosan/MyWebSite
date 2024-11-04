using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminHataLog")]
    [Authorize(Roles = "Admin")]
    public class AdminErrorLog : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminErrorLog(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        [Route("Liste")]
        public async Task<IActionResult> List()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminLogsGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<Models.AdminErrorLog> log = new();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                log.Add(new Models.AdminErrorLog()
                                {
                                    ID = Convert.ToInt16(reader["ID"]),
                                    LogType = reader["LogType"].ToString(),
                                    ErrorMessage = reader["ErrorMessage"].ToString(),
                                    IPAdress = reader["IPAdress"].ToString(),
                                    CreateDate = Convert.ToDateTime(reader["CreateDate"])
                                });
                            }
                            return View(log);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Hatalar Log Kayıtları Panelde Listeme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Hatalar Log Kayıtları Listeme İşlemi Hatası";
                }
            }
            return View();
        }
        [HttpGet]
        [Route("Sil/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminLogsDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Hatalar Log Kayıtları Silme İşlemi Başarılı";
                    return RedirectToAction("Liste", "AdminHataLog");
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Hatalar Log Kayıtları Silme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Hatalar Log Kayıtları Silme İşlemi Hatası";
                }
            }
            return RedirectToAction("Liste", "AdminHataLog");
        }
    }
}