using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminWebLog")]
    [Authorize(Roles = "Admin")]
    public class AdminLog : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminLog(IConfiguration configuration)
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
                    SqlCommand cmd = new("WebLogGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<Models.AdminLogs> log = new();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                log.Add(new Models.AdminLogs()
                                {
                                    ID = Convert.ToInt16(reader["ID"]),
                                    IPAdress = reader["IPAdress"].ToString(),
                                    UserGeo = reader["UserGeo"].ToString(),
                                    UserInfo = reader["UserInfo"].ToString(),
                                    CreateDate = Convert.ToDateTime(reader["CreateDate"])
                                });
                            }
                            return View(log);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Siteye Giren Kullanıcı Panelde Listeme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Siteye Giren Kullanıcı Listeme İşlemi Hatalı";
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
                    SqlCommand cmd = new("WebLogDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Siteye Giren Kullanıcı Silme İşlemi Başarılı";
                    return RedirectToAction("Liste", "AdminWebLog");
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Siteye Giren Kullanıcı Panelde Silme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Siteye Giren Kullanıcı Silme İşlemi Hatalı";
                }
            }
            return RedirectToAction("Liste", "AdminWebLog");
        }
    }
}