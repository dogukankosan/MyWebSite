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
        [HttpGet("Liste")]
        public async Task<IActionResult> List()
        {
            try
            {
                string sql = "SocialMediaGet";
                List<SqlParameter> parameters = new List<SqlParameter>();
                List<SocialMedia> result = await SQLCrud.ExecuteModelListAsync<SocialMedia>(
                    sql,
                    parameters,
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
                SocialMedia model = result.FirstOrDefault() ?? new SocialMedia();
                return View(model);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Sosyal Medya Panelde Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Sosyal Medya Listeleme İşlemi Hatalı";
                return View(new SocialMedia());
            }
        }
        [HttpGet("Guncelle")]
        public async Task<IActionResult> Update()
        {
            try
            {
                string sql = "SocialMediaGet";
                List<SqlParameter> parameters = new List<SqlParameter>();
                List<SocialMedia> result = await SQLCrud.ExecuteModelListAsync<SocialMedia>(
                    sql,
                    parameters,
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
                SocialMedia model = result.FirstOrDefault() ?? new SocialMedia();
                return View(model);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Sosyal Medya Panelde Güncelleme Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Sosyal Medya Güncelleme Listeleme İşlemi Hatalı";
                return View(new SocialMedia());
            }
        }
        [HttpPost("Guncelle")]
        public async Task<IActionResult> Update(SocialMedia socialMedia)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage)));
                return Json(new { success = false, errors });
            }
            try
            {
                string sql = "SocialMediaUpdate";
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@FacebookLink", socialMedia.FacebookLink ?? (object)DBNull.Value),
                    new SqlParameter("@GithubLink", socialMedia.GithubLink ?? (object)DBNull.Value),
                    new SqlParameter("@InstagramLink", socialMedia.InstagramLink ?? (object)DBNull.Value),
                    new SqlParameter("@LinkedinLink", socialMedia.LinkedinLink ?? (object)DBNull.Value)
                };
                await SQLCrud.InsertUpdateDeleteAsync(sql, parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Admin Sosyal Medya Güncelleme İşlemi Başarılı";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminSosyalMedya") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Sosyal Medya Panelde Güncelleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Sosyal Medya Güncelleme İşlemi Hatalı";
                return Json(new { success = false, message = "Admin Sosyal Medya Güncelleme İşlemi Hatalı" });
            }
        }
    }
}