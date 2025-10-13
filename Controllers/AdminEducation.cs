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
                        Years = reader["Years"].ToString(),
                        Status = Convert.ToBoolean(reader["Status"])
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
            new SqlParameter("@Years", education.Years),
            new SqlParameter("@Status", education.Status)
        };
                await SQLCrud.InsertUpdateDeleteAsync("EducationAdd", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Yeni eğitim kaydı başarıyla eklendi.";
                Response.ContentType = "application/json; charset=utf-8";
                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Liste", "AdminEgitim")
                });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Eğitim Panelde Ekleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Ekleme sırasında bir hata oluştu.";

                Response.ContentType = "application/json; charset=utf-8";
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [Route("Sil/{id:int}")]
        public async Task<IActionResult> Sil(int id)
        {
            try
            {
                List<SqlParameter> parameters = new()
        {
            new SqlParameter("@ID", id)
        };

                bool result = await SQLCrud.InsertUpdateDeleteAsync("EducationDelete", parameters);
                if (result)
                    return Json(new { success = true, message = "Eğitim başarıyla silindi." });
                else
                    return Json(new { success = false, message = "Kayıt silinemedi. SQL işlem başarısız." });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Eğitim Silme Hatası", ex.Message);
                return Json(new { success = false, message = "Beklenmedik bir hata oluştu." });
            }
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
                        Years = reader["Years"].ToString(),
                        Status = Convert.ToBoolean(reader["Status"])
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
        [Route("DurumGuncelle")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] System.Text.Json.JsonElement data)
        {
            try
            {
                int id = data.GetProperty("id").GetInt32();
                bool status = data.GetProperty("status").GetBoolean();
                List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@ID", id),
            new SqlParameter("@Status", status)
        };

                await SQLCrud.InsertUpdateDeleteAsync("EducationStatusUpdate", parameters);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Eğitim Durum Güncelleme Hatası", ex.Message);
                return Json(new { success = false });
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
                    new SqlParameter("@Years", education.Years),
                    new SqlParameter("@Status", education.Status)
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