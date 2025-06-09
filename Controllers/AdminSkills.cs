using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminBeceriler")]
    [Authorize(Roles = "Admin")]
    public class AdminSkills : Controller
    {
        [Route("Liste")]
        public async Task<IActionResult> List()
        {
            try
            {
                List<SqlParameter> parameters = null; 
                List<Skills> skills = await SQLCrud.ExecuteModelListAsync(
                    "SkillsGet",
                    parameters,
                    reader => new Skills
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        SkillName = reader["SkillName"].ToString(),
                        SkillPercent = Convert.ToByte(reader["SkillPercent"]),
                        Skillcon = reader["Skillcon"].ToString()
                    },
                    System.Data.CommandType.StoredProcedure
                );
                return View(skills);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Beceriler Panelde Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Beceriler Hatalı Listeleme İşlemi";
                return View();
            }
        }
        [HttpGet("Ekle")]
        public IActionResult Add() => View();

        [HttpPost("Ekle")]
        public async Task<IActionResult> Ekle(Skills skills)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                    .ToDictionary(k => k.Key, v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage)));
                return Json(new { success = false, errors });
            }
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@SkillName", skills.SkillName ?? (object)DBNull.Value),
                    new SqlParameter("@SkillPercent", skills.SkillPercent),
                    new SqlParameter("@Skillcon", skills.Skillcon ?? (object)DBNull.Value)
                };
                await SQLCrud.InsertUpdateDeleteAsync("SkillsAdd", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Admin Beceri Başarılı Ekleme İşlemi";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminBeceriler") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Beceriler Panelde Ekleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Beceri Hatalı Ekleme İşlemi";
                return Json(new { success = false });
            }
        }
        [HttpGet("Sil/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>  { new SqlParameter("@ID", id) };
                await SQLCrud.InsertUpdateDeleteAsync("SkillsDelete", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Admin Beceriler Başarılı Silme İşlemi";
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Beceriler Panelde Silme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Beceriler Hatalı Silme İşlemi";
            }
            return RedirectToAction("Liste", "AdminBeceriler");
        }
        [HttpGet("Guncelle/{id:int}")]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter> { new SqlParameter("@ID", id) };
                var skills = await SQLCrud.ExecuteModelListAsync("SkillsGetByID", parameters, reader => new Skills
                {
                    ID = Convert.ToByte(reader["ID"]),
                    SkillName = reader["SkillName"].ToString(),
                    SkillPercent = Convert.ToByte(reader["SkillPercent"]),
                    Skillcon = reader["Skillcon"].ToString()
                });
                return View(skills.FirstOrDefault() ?? new Skills());
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Beceriler Panelde Güncelleme Listesi Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Beceriler Hatalı Güncelleme Listesi İşlemi";
                return View(new Skills());
            }
        }
        [HttpPost("Guncelle")]
        public async Task<IActionResult> Update(Skills skills)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string,string> errors = ModelState.Where(x => x.Value.Errors.Any())
                    .ToDictionary(k => k.Key, v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage)));
                return Json(new { success = false, errors });
            }
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ID", skills.ID),
                    new SqlParameter("@SkillName", skills.SkillName ?? (object)DBNull.Value),
                    new SqlParameter("@SkillPercent", skills.SkillPercent),
                    new SqlParameter("@Skillcon", skills.Skillcon ?? (object)DBNull.Value)
                };
                await SQLCrud.InsertUpdateDeleteAsync("SkillsUpdate", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Admin Beceriler Başarılı Güncelleme İşlemi";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminBeceriler") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Beceriler Panelde Güncelleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Beceriler Hatalı Güncelleme İşlemi";
                return View();
            }
        }
    }
}