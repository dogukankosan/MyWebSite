using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminIsHayati")]
    [Authorize(Roles = "Admin")]
    public class AdminWorkLife : Controller
    {
        [HttpGet("Liste")]
        public async Task<IActionResult> List()
        {
            try
            {
                List<Jobs> jobs = await SQLCrud.ExecuteModelListAsync(
                    "JobsGet",
                    null,
                    reader => new Jobs
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        JobName = reader["JobName"].ToString(),
                        JobTitle = reader["JobTitle"].ToString(),
                        JobYears = reader["JobYears"].ToString(),
                        JobAbout = reader["JobAbout"].ToString()
                    },
                    System.Data.CommandType.StoredProcedure
                );
                return View(jobs);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin İş Hayatı Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Listeleme işlemi sırasında hata oluştu.";
                return View();
            }
        }
        [HttpGet("Ekle")]
        public IActionResult Add() => View();

        [HttpPost("Ekle")]
        public async Task<IActionResult> Add(Jobs jobs)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelState.ToDictionary(k => k.Key, v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage))) });
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@JobName", jobs.JobName),
                    new SqlParameter("@JobTitle", jobs.JobTitle),
                    new SqlParameter("@JobYears", jobs.JobYears),
                    new SqlParameter("@JobAbout", jobs.JobAbout)
                };
                await SQLCrud.InsertUpdateDeleteAsync("JobsAdd", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "İş hayatı başarıyla eklendi.";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminIsHayati") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin İş Hayatı Ekleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Ekleme sırasında bir hata oluştu.";
                return View();
            }
        }
        [HttpGet("Sil/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter> { new SqlParameter("@ID", id) };
                await SQLCrud.InsertUpdateDeleteAsync("JobsDelete", parameters);

                TempData["Type"] = "success";
                TempData["Message"] = "Silme işlemi başarılı.";
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin İş Hayatı Silme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Silme işlemi sırasında bir hata oluştu.";
            }
            return RedirectToAction("Liste", "AdminIsHayati");
        }
        [HttpGet("Guncelle/{id:int}")]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter> { new SqlParameter("@ID", id) };
                List<Jobs> jobsList = await SQLCrud.ExecuteModelListAsync(
                    "JobsGetByID",
                    parameters,
                    reader => new Jobs
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        JobName = reader["JobName"].ToString(),
                        JobTitle = reader["JobTitle"].ToString(),
                        JobYears = reader["JobYears"].ToString(),
                        JobAbout = reader["JobAbout"].ToString()
                    },
                    System.Data.CommandType.StoredProcedure
                );
                Jobs job = jobsList.FirstOrDefault();
                if (job == null)
                    return RedirectToAction("Liste", "AdminIsHayati");
                return View(job);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin İş Hayatı Güncelleme Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Güncelleme listesi yüklenemedi.";
                return View();
            }
        }
        [HttpPost("Guncelle")]
        public async Task<IActionResult> Update(Jobs jobs)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelState.ToDictionary(k => k.Key, v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage))) });
            try
            {
                List<SqlParameter> parameters =new List<SqlParameter>
                {
                    new SqlParameter("@ID", jobs.ID),
                    new SqlParameter("@JobName", jobs.JobName),
                    new SqlParameter("@JobTitle", jobs.JobTitle),
                    new SqlParameter("@JobYears", jobs.JobYears),
                    new SqlParameter("@JobAbout", jobs.JobAbout)
                };
                await SQLCrud.InsertUpdateDeleteAsync("JobsUpdate", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Güncelleme işlemi başarılı.";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminIsHayati") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin İş Hayatı Güncelleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Güncelleme sırasında bir hata oluştu.";
                return View();
            }
        }
    }
}