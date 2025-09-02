using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
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
                if (ModelState.ContainsKey("ContactName"))
                    errors["ContactName"] = string.Join(", ", ModelState["ContactName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("ContactMail"))
                    errors["ContactMail"] = string.Join(", ", ModelState["ContactMail"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("ContactPhone"))
                    errors["ContactPhone"] = string.Join(", ", ModelState["ContactPhone"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("ContactSubject"))
                    errors["ContactSubject"] = string.Join(", ", ModelState["ContactSubject"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("ContactMessage"))
                    errors["ContactMessage"] = string.Join(", ", ModelState["ContactMessage"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            try
            {
                string geo = "";
                using (HttpClient client = new())
                {
                    string response = await client.GetStringAsync($"http://ip-api.com/json/{HttpContext.Connection.RemoteIpAddress}");
                    JObject json = JObject.Parse(response);
                    geo = json["city"] + ", " + json["country"];
                }

                string sql = "ContactAdd";
                List<SqlParameter> parameters = new List<SqlParameter>
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
                return Json(new { success = true, message = "Mesajınız Başarıyla Gönderilmiştir" });
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
    }
}