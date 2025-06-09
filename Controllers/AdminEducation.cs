using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminEgitim")]
    [Authorize(Roles = "Admin")]
    public class AdminEducation : Controller
    {
        [Route("Liste")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                List<Education> educationList = await SQLCrud.ExecuteModelListAsync("EducationGet", null, delegate (SqlDataReader reader)
                {
                    Education model = new Education
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        SchoolName = reader["SchoolName"].ToString(),
                        SectionName = reader["SectionName"].ToString(),
                        Years = reader["Years"].ToString()
                    };
                    return model;
                });
                return View(educationList);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Eğitim Panelde Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Eğitim Hatalı Listeleme İşlemi";
                return View(new List<Education>());
            }
        }
        [Route("Ekle")]
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }
        [Route("Ekle")]
        [HttpPost]
        public async Task<IActionResult> Add(Education education)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = ModelState
                    .Where(e => e.Value.Errors.Any())
                    .ToDictionary(k => k.Key, v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage)));

                return Json(new { success = false, errors });
            }
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@SchoolName", education.SchoolName),
                    new SqlParameter("@SectionName", education.SectionName),
                    new SqlParameter("@Years", education.Years)
                };
                await SQLCrud.InsertUpdateDeleteAsync("EducationAdd", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Admin Eğitim Başarılı Ekleme İşlemi";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminEgitim") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Eğitim Panelde Ekleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Eğitim Hatalı Ekleme İşlemi";
                return View();
            }
        }
        [Route("Sil/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ID", id)
                };
                await SQLCrud.InsertUpdateDeleteAsync("EducationDelete", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Admin Eğitim Başarılı Silme İşlemi";
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Eğitim Panelde Silme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Eğitim Hatalı Silme İşlemi";
            }
            return RedirectToAction("Liste", "AdminEgitim");
        }
        [Route("Guncelle/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ID", id)
                };
                List<Education> result = await SQLCrud.ExecuteModelListAsync<Education>(
                    "EducationGetByID",
                    parameters,
                    reader => new Education
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        SchoolName = reader["SchoolName"].ToString(),
                        SectionName = reader["SectionName"].ToString(),
                        Years = reader["Years"].ToString()
                    },
                    CommandType.StoredProcedure
                );
                Education education = result.FirstOrDefault();
                if (education == null)
                    return RedirectToAction("Liste", "AdminEgitim");
                return View(education);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Eğitim Panelde Güncelleme Listesi Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Eğitim Hatalı Güncelleme Listesi İşlemi";
                return View();
            }
        }
        [Route("Guncelle")]
        [HttpPost]
        public async Task<IActionResult> Update(Education education)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = ModelState
                    .Where(e => e.Value.Errors.Any())
                    .ToDictionary(k => k.Key, v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage)));
                return Json(new { success = false, errors });
            }
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ID", education.ID),
                    new SqlParameter("@SchoolName", education.SchoolName),
                    new SqlParameter("@SectionName", education.SectionName),
                    new SqlParameter("@Years", education.Years)
                };
                await SQLCrud.InsertUpdateDeleteAsync("EducationUpdate", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Admin Eğitim Başarılı Güncelleme İşlemi";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminEgitim") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Eğitim Panelde Güncelleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Eğitim Hatalı Güncelleme İşlemi";
                return View();
            }
        }
    }
}