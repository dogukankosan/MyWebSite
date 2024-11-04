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
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            SocialMedia cs1 = new();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("SocialMediaGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                cs1.ID = Convert.ToByte(reader["ID"]);
                                cs1.FacebookLink = reader["FacebookLink"].ToString();
                                cs1.GithubLink = reader["GithubLink"].ToString();
                                cs1.InstagramLink = reader["InstagramLink"].ToString();
                                cs1.LinkedinLink = reader["LinkedinLink"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Anasayfada Sosyal Medya Listeleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                }
            }
            ViewBag.FacebookLink = cs1.FacebookLink;
            ViewBag.GithubLink = cs1.GithubLink;
            ViewBag.InstagramLink = cs1.InstagramLink;
            ViewBag.LinkedinLink = cs1.LinkedinLink;
            About cs = new();
            string base64Image1 = "", base64Image2 = "";
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    using (SqlCommand cmd = new("AboutGet", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    if (!Convert.IsDBNull(reader["Picture1"]))
                                    {
                                        byte[] imageBytes = (byte[])reader["Picture1"];
                                        base64Image1 = Convert.ToBase64String(imageBytes);
                                    }
                                    if (!Convert.IsDBNull(reader["Picture2"]))
                                    {
                                        byte[] imageBytes = (byte[])reader["Picture2"];
                                        base64Image2 = Convert.ToBase64String(imageBytes);
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
                                }
                            }
                        }
                    }
                    ViewBag.Picture1 = $"data:image/jpeg;base64,{base64Image1}";
                    ViewBag.Picture2 = $"data:image/jpeg;base64,{base64Image2}";
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
                    await Logging.LogAdd("Anasayfada Hakkında Listeleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                }
            }


            string geo = "";
            using (HttpClient client = new HttpClient())
            {
                string response = await client.GetStringAsync($"http://ip-api.com/json/{HttpContext.Connection.RemoteIpAddress.ToString()}");
                JObject json = JObject.Parse(response);
                geo = json["city"] + ", " + json["country"];
            }
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("WebLogAdd", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@IPAdress", HttpContext.Connection.RemoteIpAddress.ToString());
                    cmd.Parameters.AddWithValue("@UserGeo", geo);
                    cmd.Parameters.AddWithValue("@UserInfo", Request.Headers["User-Agent"].ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Siteye Giren Kullanıcı IP Alma İşlemi Hatalı", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                }
            }
            return View();
        }
        [HttpPost]
        public async Task<JsonResult> ContactAdd(Contacts c)
        {
            c.UserGeo = "test";
            c.IPAdress = "test";
            c.UserInfo = "test";
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
            string geo = "";
            using (HttpClient client = new())
            {
                string response = await client.GetStringAsync($"http://ip-api.com/json/{HttpContext.Connection.RemoteIpAddress.ToString()}");
                JObject json = JObject.Parse(response);
                geo = json["city"] + ", " + json["country"];
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("ContactAdd", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ContactName", c.ContactName);
                    cmd.Parameters.AddWithValue("@ContactMail", c.ContactMail);
                    cmd.Parameters.AddWithValue("@ContactPhone", c.ContactPhone);
                    cmd.Parameters.AddWithValue("@ContactSubject", c.ContactSubject);
                    cmd.Parameters.AddWithValue("@ContactMessage", c.ContactMessage);
                    cmd.Parameters.AddWithValue("@IPAdress", HttpContext.Connection.RemoteIpAddress.ToString());
                    cmd.Parameters.AddWithValue("@UserGeo", geo);
                    cmd.Parameters.AddWithValue("@UserInfo", Request.Headers["User-Agent"].ToString());
                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("İletişim Gönderme İşlemi Hatalı", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress?.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "İletişim Gönderme İşlemi Hatalı";
                }
            }
            return Json(new { success = true, message = "Mesajınız Başarıyla Gönderilmiştir" });
        }
        public IActionResult Error()
        {
            return View();
        }
    }
}