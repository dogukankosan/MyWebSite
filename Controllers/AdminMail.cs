using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminMail")]
    public class AdminMail : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminMail(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [HttpGet]
        [Route("Liste")]
        public async Task<IActionResult> List()
        {
            Models.AdminMail cs = new();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminMailGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                cs.ID = Convert.ToByte(reader["ID"]);
                                cs.MailAdress = reader["MailAdress"].ToString();
                                cs.MailPassword = reader["MailPassword"].ToString();
                                cs.ServerName = reader["ServerName"].ToString();
                                cs.MailPort = Convert.ToInt32(reader["MailPort"]);
                                cs.IsSSL = Convert.ToBoolean(reader["IsSSL"]);
                            }
                            return View(cs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Mail Panelde Listeleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Mail Hatalı Listeleme İşlemi";
                }
            }
            return View(cs);
        }
        [Route("Guncelle/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            Models.AdminMail cs = new();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminMailGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                cs.ID = Convert.ToByte(reader["ID"]);
                                cs.MailAdress = reader["MailAdress"].ToString();
                                cs.MailPassword = reader["MailPassword"].ToString();
                                cs.ServerName = reader["ServerName"].ToString();
                                cs.MailPort = Convert.ToInt32(reader["MailPort"]);
                                cs.IsSSL = Convert.ToBoolean(reader["IsSSL"]);
                            }
                            return View(cs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Mail Panelde Güncelleme Listeleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Mail Hatalı Güncelleme Listeleme İşlemi";
                }
            }
            return View(cs);
        }
        [Route("Guncelle")]
        [HttpPost]
        public async Task<IActionResult> Update(Models.AdminMail adminMail)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("MailAdress"))
                    errors["MailAdress"] = string.Join(", ", ModelState["MailAdress"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("MailPassword"))
                    errors["MailPassword"] = string.Join(", ", ModelState["MailPassword"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("ServerName"))
                    errors["ServerName"] = string.Join(", ", ModelState["ServerName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("MailPort"))
                    errors["MailPort"] = string.Join(", ", ModelState["MailPort"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("IsSSL"))
                    errors["IsSSL"] = string.Join(", ", ModelState["IsSSL"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminMailUpdate", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MailAdress", adminMail.MailAdress);
                    cmd.Parameters.AddWithValue("@MailPassword", adminMail.MailPassword);
                    cmd.Parameters.AddWithValue("@ServerName", adminMail.ServerName);
                    cmd.Parameters.AddWithValue("@MailPort", adminMail.MailPort);
                    cmd.Parameters.AddWithValue("@IsSSL", adminMail.IsSSL);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Mail Başarılı Güncelleme İşlemi";
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Mail Panelde Güncelleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Mail Hatalı Güncelleme İşlemi";
                }
            }
            return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminMail") });
        }
        [HttpGet]
        [Route("TestMail")]
        public async Task<IActionResult> TestMail()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            try
            {
                MailSender ms = new MailSender(connectionString);
                await ms.SendMail("Test Maili Web Siten", "Web Sitenden Test Maili Gönderme İşlemi Başarılı", HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Type"] = "success";
                TempData["Message"] = "Admin Mail Test Mail Gönderme Başarılı Mail Adresinizden Maili Kontrol Ediniz";
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Mail Panel Hatalı Test Maili Gönderme İşlemi Hatalı", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Mail Hatalı Test Maili Gönderme İşlemi Hatalı";
            }
            return RedirectToAction("Liste", "AdminMail");
        }
    }
}