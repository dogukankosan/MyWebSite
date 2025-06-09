using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Security.Claims;

namespace MyWebSite.Controllers
{
    [Route("AdminGiris")]
    public class AdminLogin : Controller
    {
        [Route("Panel")]
        [HttpGet]
        public async Task<IActionResult> Panel()
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@IPAdress", HttpContext.Connection.RemoteIpAddress?.ToString())
                };
               bool status= await SQLCrud.InsertUpdateDeleteAsync("AdminLoginErrorGet", parameters);
               if (!status)
                   throw new Exception();
                return View();
            }
            catch (Exception ex)
            {
                MailSender mailSender = new MailSender();
                await mailSender.SendMail("KULLANICI 3 DEFA WEB SİTENİZE GİRİŞ YAPMAYA ÇALIŞTI",
                    $"KULLANICININ SİSTEME GİRİŞİ KİTLENDİ. 3 DEFA HATALI GİRİŞ YAPILDI. İP: {HttpContext.Connection.RemoteIpAddress}");
                await Logging.LogAdd("Admin Giriş Panelde Hatalı Giriş", "3 defa üst üste hatalı giriş yapıldı.");
                return NotFound();
            }
        }
        [Route("Panel")]
        [HttpPost]
        public async Task<IActionResult> Panel(Login model)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(x => x.Key, x => string.Join(", ", x.Value.Errors.Select(e => e.ErrorMessage)));
                return Json(new { success = false, errors });
            }
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@AdminUserName", model.UserName)
                };
                string result = await SQLCrud.ExecuteScalarAsync("AdminLoginSignUp", parameters, string.Empty);
                bool isPasswordCorrect = HashingControl.VerifyPassword(model.Password, result);
                if (isPasswordCorrect)
                {
                    List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.UserName),
                        new Claim(ClaimTypes.Role, "Admin")
                    };
                    ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    AuthenticationProperties properties = new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                        AllowRefresh = true
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), properties);
                    return Json(new { success = true, redirectUrl = Url.Action("Anasayfa", "Admin") });
                }
                else
                {
                    TempData["Message"] = "error1";
                    string geo = string.Empty;
                    using (HttpClient client = new HttpClient())
                    {
                        string response = await client.GetStringAsync($"http://ip-api.com/json/{HttpContext.Connection.RemoteIpAddress}");
                        JObject json = JObject.Parse(response);
                        geo = json["city"] + ", " + json["country"];
                    }
                    List<SqlParameter> insertParams = new List<SqlParameter>
                    {
                        new SqlParameter("@IPAdress", HttpContext.Connection.RemoteIpAddress?.ToString()),
                        new SqlParameter("@Geo", geo),
                        new SqlParameter("@UserInfo", Request.Headers["User-Agent"].ToString())
                    };
                    await SQLCrud.InsertUpdateDeleteAsync("AdminLoginInsert", insertParams);
                    MailSender mailSender = new MailSender();
                    await mailSender.SendMail("WEB SİTEMDE HATALI GİRİŞ", $"Hatalı giriş tespit edildi. IP: {HttpContext.Connection.RemoteIpAddress}");
                    await Logging.LogAdd("Admin Giriş Panelde Hatalı Giriş", "Hatalı kullanıcı bilgileriyle giriş denemesi yapıldı.");
                    return Json(new
                    {
                        success = false,
                        locked = true,
                        alertMessage = "Hatalı kullanıcı adı veya şifre girildi."
                    });

                }
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Giriş Panelde Hata", ex.Message);
                TempData["Message"] = "Beklenmedik bir hata oluştu.";
                return View();
            }
        }
    }
}
