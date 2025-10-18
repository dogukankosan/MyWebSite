using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminCv")]
    [Authorize(Roles = "Admin")]
    public class AdminCVController : Controller
    {
        [Route("Yonetim")]
        [HttpGet]
        public async Task<IActionResult> Yonetim()
        {
            string sql = "CvDownloadLogGet";
            var logList = await SQLCrud.ExecuteModelListAsync<CvDownloadLog>(
                sql,
                null,
                reader => new CvDownloadLog
                {
                    ID = Convert.ToInt32(reader["ID"]),
                    IPAdress = reader["IPAdress"].ToString(),
                    UserGeo = reader["UserGeo"].ToString(),
                    UserInfo = reader["UserInfo"].ToString(),
                    DownloadDate = Convert.ToDateTime(reader["DownloadDate"])
                },
                CommandType.StoredProcedure
            );
            string cvPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cv");
            if (!Directory.Exists(cvPath))
                Directory.CreateDirectory(cvPath);
            var cvList = await SQLCrud.ExecuteModelListAsync<MyCV>(
                "MyCVGet",
                null,
                reader => new MyCV
                {
                    ID = Convert.ToInt32(reader["ID"]),
                    CV = reader["CV"] as byte[]
                },
                CommandType.StoredProcedure
            );
            MyCV cv = cvList.FirstOrDefault();
            if (cv?.CV != null && cv.CV.Length > 0)
            {
                await System.IO.File.WriteAllBytesAsync(Path.Combine(cvPath, "cv.pdf"), cv.CV);
                ViewBag.CvVar = true;
            }
            else
            {
                ViewBag.CvVar = false;
            }

            return View(logList);
        }

        [Route("Guncelle")]
        [HttpPost]
        public async Task<IActionResult> CvGuncelle(IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
                return Json(new { success = false, message = "Lütfen bir dosya seçin." });
            if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
                return Json(new { success = false, message = "Sadece .pdf dosyalar yüklenebilir." });
            try
            {
                byte[] pdfBytes;
                using (MemoryStream ms = new MemoryStream())
                {
                    await pdfFile.CopyToAsync(ms);
                    pdfBytes = ms.ToArray();
                }
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@CV", SqlDbType.VarBinary) { Value = pdfBytes }
                };
                bool result = await SQLCrud.InsertUpdateDeleteAsync("MyCVUpdate", parameters);
                if (result)
                {
                    string cvPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "cv");
                    if (!Directory.Exists(cvPath))
                        Directory.CreateDirectory(cvPath);
                    await System.IO.File.WriteAllBytesAsync(Path.Combine(cvPath, "cv.pdf"), pdfBytes);

                    return Json(new { success = true, message = "CV başarıyla güncellendi." });
                }
                else
                {
                    return Json(new { success = false, message = "Veritabanı güncelleme başarısız." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }
        [Route("MevcutCv")]
        [HttpGet]
        public async Task<IActionResult> MevcutCv()
        {
            string sql = "MyCVGet";
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
            MyCV cv = cvList.FirstOrDefault();
            if (cv == null || cv.CV == null)
                return NotFound("CV bulunamadı");
            string hostName = Request.Host.Host.Replace("www.", "");
            string cleanName = hostName.Contains(".") ? hostName.Split('.')[0] : hostName;
            string fileName = $"{cleanName}CV.pdf";
            return File(cv.CV, "application/pdf", fileName);
        }
        [Route("LogSil/{id}")]
        [HttpPost]
        public async Task<IActionResult> LogSil(int id)
        {
            try
            {
                string sql = "CvDownloadLogDelete";
                var parameters = new List<SqlParameter> { new SqlParameter("@ID", id) };
                await SQLCrud.InsertUpdateDeleteAsync(sql, parameters);
                TempData["Message"] = "Log başarıyla silindi.";
                TempData["Type"] = "success";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Log silme hatası: " + ex.Message;
                TempData["Type"] = "error";
            }
            return RedirectToAction("Yonetim");
        }
    }
}