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
                bool status = await SQLCrud.InsertUpdateDeleteAsync("AdminLoginErrorGet", parameters);
                if (!status)
                    throw new Exception();
                return View();
            }
            catch (Exception ex)
            {
                string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                string geo = string.Empty;
                try
                {
                    using (HttpClient client = new())
                    {
                        string response = await client.GetStringAsync($"http://ip-api.com/json/{ip}");
                        JObject json = JObject.Parse(response);
                        geo = $"{json["city"]}, {json["country"]}";
                    }
                }
                catch
                {
                    geo = "Bilinmiyor";
                }
                string subject = "🚫 Yönetici Girişi Engellendi - 3 Defa Hatalı Deneme";
                string body = $@"
<html>
<body style='font-family:Segoe UI,Arial,sans-serif;background-color:#f7f9fc;padding:20px;'>
  <div style='max-width:650px;margin:auto;background:#fff;border-radius:8px;
              box-shadow:0 4px 12px rgba(0,0,0,0.1);overflow:hidden;'>
    <div style='background-color:#d6336c;padding:16px 24px;color:#fff;font-size:18px;font-weight:600;'>
        🚫 Yönetici Girişi Engellendi
    </div>
    <div style='padding:24px;color:#333;'>
      <p>Bir kullanıcı <b>3 defa hatalı giriş denemesi</b> yaptığı için IP geçici olarak engellendi.</p>
      <hr style='border:none;border-top:1px solid #eee;margin:12px 0;'/>

      <table style='width:100%;font-size:15px;line-height:1.6;'>
        <tr><td style='width:180px;font-weight:bold;'>📍 Konum:</td><td>{geo}</td></tr>
        <tr><td style='font-weight:bold;'>🌐 IP Adresi:</td><td>{ip}</td></tr>
        <tr><td style='font-weight:bold;'>🕓 Tarih:</td><td>{DateTime.Now:dd MMMM yyyy HH:mm:ss}</td></tr>
        <tr><td style='font-weight:bold;'>⚠️ Durum:</td><td>3 defa hatalı giriş denemesi yapıldı, sistem erişimi engellendi.</td></tr>
      </table>

      <hr style='border:none;border-top:1px solid #eee;margin:12px 0;'/>

      <p style='color:#d6336c;font-weight:600;'>❗ IP adresini kontrol etmenizi öneririz. Şüpheli bir aktivite olabilir.</p>
      <p style='font-size:13px;color:#888;text-align:center;margin-top:15px;'>
        🔒 Bu uyarı <a href='https://{Request.Host}' style='color:#4096FF;text-decoration:none;'>web siteniz</a> tarafından otomatik olarak gönderilmiştir.
      </p>
    </div>
  </div>
</body>
</html>";
                MailSender mailSender = new();
                await mailSender.SendMail(subject, body);
                await Logging.LogAdd("Admin Giriş Panelde Hatalı Giriş",
                    "Kullanıcı 3 defa üst üste hatalı giriş yaptı ve sistem erişimi kilitlendi.");
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
                List<SqlParameter> parameters = new()
        {
            new SqlParameter("@AdminUserName", model.UserName)
        };
                string result = await SQLCrud.ExecuteScalarAsync("AdminLoginSignUp", parameters, string.Empty);
                bool isPasswordCorrect = HashingControl.VerifyPassword(model.Password, result);
                if (isPasswordCorrect)
                {
                    List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, model.UserName),
                new Claim(ClaimTypes.Role, "Admin")
            };
                    ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    AuthenticationProperties props = new()
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                        AllowRefresh = true
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);
                    return Json(new { success = true, redirectUrl = Url.Action("Anasayfa", "Admin") });
                }
                else
                {
                    TempData["Message"] = "error1";
                    string geo = string.Empty;
                    using (HttpClient client = new())
                    {
                        string response = await client.GetStringAsync($"http://ip-api.com/json/{HttpContext.Connection.RemoteIpAddress}");
                        JObject json = JObject.Parse(response);
                        geo = $"{json["city"]}, {json["country"]}";
                    }
                    List<SqlParameter> insertParams = new()
            {
                new SqlParameter("@IPAdress", HttpContext.Connection.RemoteIpAddress?.ToString()),
                new SqlParameter("@Geo", geo),
                new SqlParameter("@UserInfo", Request.Headers["User-Agent"].ToString())
            };
                    await SQLCrud.InsertUpdateDeleteAsync("AdminLoginInsert", insertParams);
                    string subject = "🚨 Web Sitenizde Hatalı Yönetici Girişi Tespit Edildi!";
                    string body = $@"
<html>
<body style='font-family:Segoe UI,Arial,sans-serif;background-color:#f7f9fc;padding:20px;'>
  <div style='max-width:650px;margin:auto;background:#fff;border-radius:8px;
              box-shadow:0 4px 12px rgba(0,0,0,0.1);overflow:hidden;'>
    <div style='background-color:#e03131;padding:16px 24px;color:#fff;font-size:18px;font-weight:600;'>
        🚨 Hatalı Yönetici Girişi Tespit Edildi
    </div>
    <div style='padding:24px;color:#333;'>
      <p>Admin paneline hatalı bir giriş denemesi yapıldı.</p>
      <hr style='border:none;border-top:1px solid #eee;margin:12px 0;'/>
      <table style='width:100%;font-size:15px;line-height:1.6;'>
        <tr><td style='width:180px;font-weight:bold;'>💻 Kullanıcı Adı:</td><td>{model.UserName}</td></tr>
        <tr><td style='font-weight:bold;'>📍 Konum:</td><td>{geo}</td></tr>
        <tr><td style='font-weight:bold;'>🌐 IP Adresi:</td><td>{HttpContext.Connection.RemoteIpAddress}</td></tr>
        <tr><td style='font-weight:bold;'>🕓 Tarih:</td><td>{DateTime.Now:dd MMMM yyyy HH:mm:ss}</td></tr>
      </table>
      <hr style='border:none;border-top:1px solid #eee;margin:12px 0;'/>
      <p style='color:#e03131;font-weight:600;'>❗ Bu deneme başarısız olmuştur. Şüpheli bir hareketse IP’yi kontrol ediniz.</p>
      <p style='font-size:13px;color:#888;text-align:center;margin-top:15px;'>
        🔒 Bu uyarı <a href='https://{Request.Host}' style='color:#4096FF;text-decoration:none;'>web siteniz</a> tarafından otomatik olarak gönderilmiştir.
      </p>
    </div>
  </div>
</body>
</html>";
                    MailSender mailSender = new();
                    await mailSender.SendMail(subject, body);
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