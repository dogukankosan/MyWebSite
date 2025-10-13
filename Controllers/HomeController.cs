using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            try
            {
                string sql = "SocialMediaGet";
                List<SocialMedia> socialList = await SQLCrud.ExecuteModelListAsync<SocialMedia>(
                    sql,
                    null,
                    reader => new SocialMedia
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        FacebookLink = reader["FacebookLink"].ToString(),
                        GithubLink = reader["GithubLink"].ToString(),
                        InstagramLink = reader["InstagramLink"].ToString(),
                        LinkedinLink = reader["LinkedinLink"].ToString()
                    },
                    CommandType.StoredProcedure
                );
                SocialMedia cs1 = socialList.FirstOrDefault() ?? new SocialMedia();
                ViewBag.FacebookLink = cs1.FacebookLink;
                ViewBag.GithubLink = cs1.GithubLink;
                ViewBag.InstagramLink = cs1.InstagramLink;
                ViewBag.LinkedinLink = cs1.LinkedinLink;
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfada Sosyal Medya Listeleme Hatası", ex.Message);
            }
            try
            {
                string sql = "AboutGet";
                List<About> aboutList = await SQLCrud.ExecuteModelListAsync<About>(
                    sql,
                    null,
                    reader =>
                    {
                        About cs = new About();
                        if (!Convert.IsDBNull(reader["Picture1"]))
                        {
                            byte[] imageBytes = (byte[])reader["Picture1"];
                            ViewBag.Picture1 = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
                        }
                        if (!Convert.IsDBNull(reader["Picture2"]))
                        {
                            byte[] imageBytes = (byte[])reader["Picture2"];
                            ViewBag.Picture2 = $"data:image/jpeg;base64,{Convert.ToBase64String(imageBytes)}";
                        }
                        cs.ID = reader["ID"] != DBNull.Value ? Convert.ToByte(reader["ID"]) : default;
                        cs.AboutTitle = reader["AboutTitle"]?.ToString();
                        cs.AboutDetails1 = reader["AboutDetails1"]?.ToString();
                        cs.AboutAdress = reader["AboutAdress"]?.ToString();
                        cs.AboutMail = reader["AboutMail"]?.ToString();
                        cs.AboutPhone = reader["AboutPhone"]?.ToString();
                        cs.AboutWebSite = reader["AboutWebSite"]?.ToString();
                        cs.AboutName = reader["AboutName"]?.ToString();
                        cs.AboutDetails2 = reader["AboutDetails2"]?.ToString();
                        cs.IFrameAdress = reader["IFrameAdress"]?.ToString();
                        return cs;
                    },
                    CommandType.StoredProcedure
                );
                About cs = aboutList.FirstOrDefault() ?? new About();
                ViewBag.AboutTitle = cs.AboutTitle;
                ViewBag.AboutDetails1 = cs.AboutDetails1;
                ViewBag.AboutAdress = cs.AboutAdress;
                ViewBag.AboutMail = cs.AboutMail;
                ViewBag.AboutPhone = cs.AboutPhone;
                ViewBag.AboutWebSite = cs.AboutWebSite;
                ViewBag.AboutName = cs.AboutName;
                ViewBag.AboutDetails2 = cs.AboutDetails2;
                ViewBag.IFrameAdress = cs.IFrameAdress;
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfada Hakkında Listeleme Hatası", ex.Message);
            }
            try
            {
                string geo = "";
                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync($"http://ip-api.com/json/{HttpContext.Connection.RemoteIpAddress}");
                    JObject json = JObject.Parse(response);
                    geo = json["city"] + ", " + json["country"];
                }
                string sql = "WebLogAdd";
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@IPAdress", HttpContext.Connection.RemoteIpAddress?.ToString()),
                    new SqlParameter("@UserGeo", geo),
                    new SqlParameter("@UserInfo", Request.Headers["User-Agent"].ToString())
                };
                await SQLCrud.InsertUpdateDeleteAsync(sql, parameters);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Siteye Giren Kullanıcı IP Alma İşlemi Hatalı", ex.Message);
            }

            return View();
        }
        [HttpPost]
        public async Task<JsonResult> ContactAdd(Contacts c, string website, int? MathCaptcha)
        {
            if (!string.IsNullOrEmpty(website))
            {
                await Logging.LogAdd("Spam Tespit Edildi", $"IP: {HttpContext.Connection.RemoteIpAddress}, Website field: {website}");
                return Json(new { success = false, message = "Spam tespit edildi." });
            }
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                foreach (var key in ModelState.Keys)
                {
                    if (ModelState[key].Errors.Any())
                        errors[key] = string.Join(", ", ModelState[key].Errors.Select(e => e.ErrorMessage));
                }
                return Json(new { success = false, errors });
            }
            try
            {
                string geo = "";
                using (HttpClient client = new())
                {
                    string response = await client.GetStringAsync($"http://ip-api.com/json/{HttpContext.Connection.RemoteIpAddress}");
                    JObject json = JObject.Parse(response);
                    geo = $"{json["city"]}, {json["country"]}";
                }
                string sql = "ContactAdd";
                List<SqlParameter> parameters = new()
        {
            new SqlParameter("@ContactName", c.ContactName),
            new SqlParameter("@ContactMail", c.ContactMail),
            new SqlParameter("@ContactPhone", c.ContactPhone),
            new SqlParameter("@ContactSubject", c.ContactSubject),
            new SqlParameter("@ContactMessage", c.ContactMessage),
            new SqlParameter("@IPAdress", HttpContext.Connection.RemoteIpAddress?.ToString()),
            new SqlParameter("@UserGeo", geo),
            new SqlParameter("@UserInfo", Request.Headers["User-Agent"].ToString())
        };
                await SQLCrud.InsertUpdateDeleteAsync(sql, parameters);
                string cleanedPhone = c.ContactPhone?
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace(" ", "")
                    .Replace("-", "")
                    .Replace("+90", "")
                    .Trim() ?? "";
                string whatsappLink = $"https://wa.me/90{cleanedPhone}?text=Merhaba%2C%20web%20siteniz%20üzerinden%20size%20ulaşıyorum.";
                string mailSubject = $"📩 Yeni İletişim Mesajı — {c.ContactName}";
                string mailBody = $@"
<html>
<body style='font-family:Segoe UI,Arial,sans-serif;background-color:#f7f9fc;padding:20px;'>
    <div style='max-width:650px;margin:auto;background:#fff;border-radius:8px;box-shadow:0 4px 12px rgba(0,0,0,0.1);overflow:hidden;'>
        <div style='background-color:#4096FF;padding:16px 24px;color:#fff;font-size:18px;font-weight:600;'>
            📩 Yeni İletişim Mesajı — {c.ContactName}
        </div>
        <div style='padding:24px;color:#333;'>
            <p style='margin-bottom:10px;'>Yeni bir iletişim formu gönderildi:</p>
            <hr style='border:none;border-top:1px solid #eee;margin:12px 0;'/>
            <table style='width:100%;font-size:15px;line-height:1.6;'>
                <tr><td style='width:160px;font-weight:bold;'>👤 Ad Soyad:</td><td>{c.ContactName}</td></tr>
                <tr><td style='font-weight:bold;'>📧 E-Posta:</td><td><a href='mailto:{c.ContactMail}' style='color:#4096FF;text-decoration:none;'>{c.ContactMail}</a></td></tr>
                <tr><td style='font-weight:bold;'>📞 Telefon:</td><td><a href='{whatsappLink}' target='_blank' style='color:#25D366;text-decoration:none;'>📱 {c.ContactPhone}</a></td></tr>
                <tr><td style='font-weight:bold;'>📝 Konu:</td><td>{c.ContactSubject}</td></tr>
                <tr><td style='font-weight:bold;vertical-align:top;'>💬 Mesaj:</td><td>{c.ContactMessage}</td></tr>
            </table>
            <hr style='border:none;border-top:1px solid #eee;margin:12px 0;'/>
            <table style='width:100%;font-size:14px;color:#555;'>
                <tr><td style='width:160px;font-weight:bold;'>🌍 IP:</td><td>{HttpContext.Connection.RemoteIpAddress}</td></tr>
                <tr><td style='font-weight:bold;'>📍 Konum:</td><td>{geo}</td></tr>
                <tr><td style='font-weight:bold;'>🕓 Tarih:</td><td>{DateTime.Now.ToString("dd MMMM yyyy HH:mm:ss")}</td></tr>
            </table>
            <hr style='border:none;border-top:1px solid #eee;margin:12px 0;'/>
            <p style='font-size:13px;color:#888;text-align:center;margin-top:15px;'>
                🌐 Bu mesaj <a href='https://{Request.Host}' style='color:#4096FF;text-decoration:none;'>web siteniz</a> üzerinden gönderilmiştir.
            </p>
        </div>
    </div>
</body>
</html>";
                MailSender mailSender = new();
                await mailSender.SendMail(mailSubject, mailBody);
                return Json(new { success = true, message = "Mesajınız başarıyla gönderildi." });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("İletişim Gönderme İşlemi Hatalı", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "İletişim Gönderme İşlemi Hatalı";
                return Json(new { success = false, message = "İletişim Gönderme İşlemi Hatalı" });
            }
        }
        public IActionResult Error()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> DownloadCV()
        {
            try
            {
                string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                string geo = "";
                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync($"http://ip-api.com/json/{ip}");
                    JObject json = JObject.Parse(response);
                    geo = $"{json["city"]}, {json["country"]}";
                }
                string sqlLog = "CvDownloadLogAdd";
                List<SqlParameter> logParams = new List<SqlParameter>
        {
            new SqlParameter("@IPAdress", ip ?? (object)DBNull.Value),
            new SqlParameter("@UserGeo", geo ?? (object)DBNull.Value),
            new SqlParameter("@UserInfo", Request.Headers["User-Agent"].ToString())
        };
                await SQLCrud.InsertUpdateDeleteAsync(sqlLog, logParams);
                string sql = "MyCVGet";
                byte[]? cvFile = null;
                var cvList = await SQLCrud.ExecuteModelListAsync<MyCV>(
                    sql,
                    null,
                    reader => new MyCV
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        CV = reader["CV"] as byte[]
                    },
                    CommandType.StoredProcedure
                );
                MyCV? cv = cvList.FirstOrDefault();
                if (cv == null || cv.CV == null)
                    return NotFound("CV bulunamadı");
                return File(cv.CV, "application/pdf", "CV.pdf");
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("CV indirme hatası", ex.ToString());
                return NotFound("CV indirilemedi");
            }
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("Cerez-Politikasi")]
        public async Task<IActionResult> CerezPolitikasi()
        {
            await LoadBasicAboutDataAsync();
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("Gizlilik-Politikasi")]
        public async Task<IActionResult> GizlilikPolitikasi()
        {
            await LoadBasicAboutDataAsync();
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("Blog")]
        public async Task<IActionResult> Blog()
        {
            await LoadBasicAboutDataAsync();
            return View();
        }
        private async Task LoadBasicAboutDataAsync()
        {
            try
            {
                string sqlAbout = "AboutGet";
                List<About> aboutList = await SQLCrud.ExecuteModelListAsync<About>(
                    sqlAbout,
                    null,
                    reader => new About
                    {
                        AboutName = reader["AboutName"]?.ToString(),
                        AboutMail = reader["AboutMail"]?.ToString(),
                        AboutWebSite = reader["AboutWebSite"]?.ToString()
                    },
                    CommandType.StoredProcedure
                );
                About ab = aboutList.FirstOrDefault() ?? new About();
                ViewBag.AboutName = ab.AboutName;
                ViewBag.AboutMail = ab.AboutMail;
                ViewBag.AboutWebSite = ab.AboutWebSite;
                string sqlSocial = "SocialMediaGet";
                List<SocialMedia> socialList = await SQLCrud.ExecuteModelListAsync<SocialMedia>(
                    sqlSocial,
                    null,
                    reader => new SocialMedia
                    {
                        FacebookLink = reader["FacebookLink"]?.ToString(),
                        GithubLink = reader["GithubLink"]?.ToString(),
                        InstagramLink = reader["InstagramLink"]?.ToString(),
                        LinkedinLink = reader["LinkedinLink"]?.ToString()
                    },
                    CommandType.StoredProcedure
                );
                SocialMedia sm = socialList.FirstOrDefault() ?? new SocialMedia();
                ViewBag.FacebookLink = sm.FacebookLink;
                ViewBag.GithubLink = sm.GithubLink;
                ViewBag.InstagramLink = sm.InstagramLink;
                ViewBag.LinkedinLink = sm.LinkedinLink;
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Çerez/Gizlilik sayfası bilgi yükleme hatası", ex.Message);
            }
        }
    }
}