using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminKullanici")]
    [Authorize(Roles = "Admin")]
    public class AdminUserLogin : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminUserLogin(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        [Route("Liste")]
        public async Task<IActionResult> Liste()
        {
            Login cs = new();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminLoginGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                cs.UserName = reader["AdminUserName"].ToString();
                                cs.Password = reader["Password_"].ToString();
                            }
                            return View(cs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Kullanıcı Giriş Bilgileri Panelde Listeleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Kullanıcı Giriş Bilgileri Listeleme Hatası";
                }
            }
            return View(cs);
        }
        [Route("Guncelle")]
        [HttpGet]
        public async Task<IActionResult> Update()
        {
            AdminLoginProp cs = new();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminLoginGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                cs.UserName = reader["AdminUserName"].ToString();
                                cs.Password = "***";
                            }
                            return View(cs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Kullanıcı Giriş Bilgileri Panelde Güncelleme Listeleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Kullanıcı Giriş Bilgileri Güncelleme Listesi Hatası";
                }
            }
            return View(cs);
        }
        [Route("Guncelle")]
        [HttpPost]
        public async Task<IActionResult> Update(AdminLoginProp login)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();

                if (ModelState.ContainsKey("UserName"))
                    errors["UserName"] = string.Join(", ", ModelState["UserName"].Errors.Select(e => e.ErrorMessage));

                if (ModelState.ContainsKey("Password"))
                    errors["Password"] = string.Join(", ", ModelState["Password"].Errors.Select(e => e.ErrorMessage));

                return Json(new { success = false, errors });
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminLoginUpdate", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AdminUserName", login.UserName);
                    cmd.Parameters.AddWithValue("@Password_", HashingControl.HashPassword(login.Password));
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Giriş Bilgileri Başarılı Güncelleme İşlemi";
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Kullanıcı Giriş Bilgileri Panelde Güncelleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Kullanıcı Giriş Bilgileri Güncelleme Hatası";
                }
            }
            return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminKullanici") });
        }
    }
}