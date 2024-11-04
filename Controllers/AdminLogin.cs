using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;

namespace MyWebSite.Controllers
{
    [Route("AdminGiris")]
    public class AdminLogin : Controller
    {
        private readonly string _connectionString;
        public AdminLogin(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        [Route("Panel")]
        [HttpGet]
        public async Task<IActionResult> Panel()
        {
            using (SqlConnection con = new(_connectionString))
            {
                try
                {
                    using (SqlCommand cmd = new("AdminLoginErrorGet", con))
                    {
                        await con.OpenAsync();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@IPAdress", HttpContext.Connection.RemoteIpAddress.ToString());
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    MailSender mail = new(_connectionString);
                    await mail.SendMail("KULLANICI 3 DEFA WEB SİTENİZE GİRİŞ YAPMAYA ÇALIŞTI", $"KULLANCININ SİSTEME GİRİŞİ KİTLENDİ 3 DEFA HATALI GİRİŞ YAPILDI KULLANICININ İP ADRESİ:  {HttpContext.Connection.RemoteIpAddress.ToString()} ");
                    await Logging.LogAdd("Admin Giriş Panelde Hatalı Giriş", "KULLANICI BİRDEN FAZLA KES GİRİŞ YAPMAYA ÇALIŞIYOR KİTLENDİ", _connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    return NotFound();
                }
                return View();
            }
        }
        [Route("Panel")]
        //[ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Panel(Login model)
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
            try
            {
                using (SqlConnection con = new(_connectionString))
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new("AdminLoginSignUp", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AdminUserName", model.UserName);
                        string result = (string)await cmd.ExecuteScalarAsync();
                        bool isPasswordCorrect = HashingControl.VerifyPassword(model.Password, result);
                        if (isPasswordCorrect)
                        {
                            var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, model.UserName),
                                    new Claim(ClaimTypes.Role, "Admin")
                                 };
                            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            AuthenticationProperties authProperties = new AuthenticationProperties
                            {
                                IsPersistent = false,
                                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                                AllowRefresh = true
                            };
                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                            return Json(new { success = true, redirectUrl = Url.Action("Anasayfa", "Admin") });
                        }
                        else
                        {
                            TempData["Message"] = "error1";
                            string geo = "";
                            using (HttpClient client = new ())
                            {
                                string response = await client.GetStringAsync($"http://ip-api.com/json/{HttpContext.Connection.RemoteIpAddress.ToString()}");
                                JObject json = JObject.Parse(response);
                                geo = json["city"] + ", " + json["country"];
                            }
                            using (SqlCommand cmd1 = new("AdminLoginInsert", con))
                            {
                                cmd1.CommandType = CommandType.StoredProcedure;
                                cmd1.Parameters.AddWithValue("@IPAdress", HttpContext.Connection.RemoteIpAddress.ToString());
                                cmd1.Parameters.AddWithValue("@Geo", geo);
                                cmd1.Parameters.AddWithValue("@UserInfo", Request.Headers["User-Agent"].ToString());
                                await cmd1.ExecuteNonQueryAsync();
                            }
                            MailSender mail = new(_connectionString);
                            await mail.SendMail("WEB SİTEMDE HATALI GİRİŞ", $"KULLANICI HATALI GİRİŞ YAPILDI ADMİN PANELDE İP ADRESİ: {HttpContext.Connection.RemoteIpAddress.ToString()} ");
                            await Logging.LogAdd("Admin Giriş Panelde Hatalı Giriş", "HATALI GİRİŞ YAPILDI", _connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                            ModelState.AddModelError(string.Empty, "Geçersiz giriş.");
                            return Json(new { success = false, locked = true });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                await Logging.LogAdd("Admin Giriş Panelde Hatalı Giriş", ex.Message, _connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                TempData["Message"] = "Beklenmedik bir hata oluştu. Lütfen tekrar deneyin.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Beklenmedik bir hata oluştu. Lütfen tekrar deneyin.";
                await Logging.LogAdd("Admin Giriş Panelde Hatalı Giriş", ex.Message, _connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
            }
            return View();
        }
    }
}