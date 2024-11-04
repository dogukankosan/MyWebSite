using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminHataliGiris")]
    [Authorize(Roles = "Admin")]
    public class AdminErrorLogin : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminErrorLogin(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("Liste")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminLoginErrorGetAll", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<Models.AdminErrorLogin> errorLogin = new();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                errorLogin.Add(new Models.AdminErrorLogin()
                                {
                                    ID = Convert.ToInt16(reader["ID"]),
                                    IPAdress = reader["IPAdress"].ToString(),
                                    Geo = reader["Geo"].ToString(),
                                    UserInfo = reader["UserInfo"].ToString(),
                                    CreateDate = Convert.ToDateTime(reader["CreateDate"]),
                                });
                            }
                            return View(errorLogin);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Hatalı Giriş Bilgileri Listeleme Hatalı", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Hatalı Giriş Bilgileri Listeleme Hatalı";
                }
                return View();
            }
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
                    SqlCommand cmd = new("AdminLoginErrorDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Hatalı Giriş Başarılı Silme İşlemi";
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Hatalı Giriş Bilgileri Silme Hatalı", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Hatalı Giriş Hatalı Silme İşlemi";
                }
            }
            return RedirectToAction("Liste", "AdminHataliGiris");
        }
    }
}