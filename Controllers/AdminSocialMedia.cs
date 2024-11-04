using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminSosyalMedya")]
    [Authorize(Roles = "Admin")]
    public class AdminSocialMedia : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminSocialMedia(IConfiguration configuration)
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
                    SocialMedia cs = new();
                    await con.OpenAsync();
                    SqlCommand cmd = new("SocialMediaGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                cs.ID = Convert.ToByte(reader["ID"]);
                                cs.FacebookLink = reader["FacebookLink"].ToString();
                                cs.GithubLink = reader["GithubLink"].ToString();
                                cs.InstagramLink = reader["InstagramLink"].ToString();
                                cs.LinkedinLink = reader["LinkedinLink"].ToString();
                            }
                            return View(cs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Sosyal Medya Panelde Listeleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Sosyal Medya Listeleme İşlemi Hatalı";
                }
            }
            return View();
        }
        [Route("Guncelle")]
        [HttpGet]
        public async Task<IActionResult> Update()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    SocialMedia cs = new();
                    await con.OpenAsync();
                    SqlCommand cmd = new("SocialMediaGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                cs.ID = Convert.ToByte(reader["ID"]);
                                cs.FacebookLink = reader["FacebookLink"].ToString();
                                cs.GithubLink = reader["GithubLink"].ToString();
                                cs.InstagramLink = reader["InstagramLink"].ToString();
                                cs.LinkedinLink = reader["LinkedinLink"].ToString();
                            }
                            return View(cs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Sosyal Medya Panelde Güncelleme Listeleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Sosyal Medya Güncelleme Listeleme İşlemi Hatalı";
                }
            }
            return View();
        }
        [Route("Guncelle")]
        [HttpPost]
        public async Task<IActionResult> Update(SocialMedia socialMedia)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("FacebookLink"))
                    errors["FacebookLink"] = string.Join(", ", ModelState["FacebookLink"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("GithubLink"))
                    errors["GithubLink"] = string.Join(", ", ModelState["GithubLink"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("InstagramLink"))
                    errors["InstagramLink"] = string.Join(", ", ModelState["InstagramLink"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("LinkedinLink"))
                    errors["LinkedinLink"] = string.Join(", ", ModelState["LinkedinLink"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("SocialMediaUpdate", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@FacebookLink", socialMedia.FacebookLink);
                    cmd.Parameters.AddWithValue("@GithubLink", socialMedia.GithubLink);
                    cmd.Parameters.AddWithValue("@InstagramLink", socialMedia.InstagramLink);
                    cmd.Parameters.AddWithValue("@LinkedinLink", socialMedia.LinkedinLink);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Sosyal Medya Güncelleme İşlemi Başarılı";
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Sosyal Medya Panelde Güncelleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Sosyal Medya Güncelleme İşlemi Hatalı";
                }
            }
            return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminSosyalMedya") });
        }
    }
}