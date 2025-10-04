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
            return View(logList); 
        }
        [Route("Guncelle")]
        [HttpPost]
        public async Task<IActionResult> CvGuncelle(IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                TempData["Message"] = "Lütfen bir dosya seçin.";
                TempData["Type"] = "error";
                return RedirectToAction("Yonetim");
            }
            if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
            {
                TempData["Message"] = "Sadece .pdf uzantılı dosyalar yüklenebilir.";
                TempData["Type"] = "error";
                return RedirectToAction("Yonetim");
            }
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
                    TempData["Message"] = "CV başarıyla güncellendi.";
                    TempData["Type"] = "success";
                }
                else
                {
                    TempData["Message"] = "CV güncelleme işlemi başarısız oldu.";
                    TempData["Type"] = "error";
                }
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Hata oluştu: " + ex.Message;
                TempData["Type"] = "error";
            }
            return RedirectToAction("Yonetim");
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
            MyCV? cv = cvList.FirstOrDefault();
            if (cv == null || cv.CV == null)
                return NotFound("CV bulunamadı");
            return File(cv.CV, "application/pdf", "CV.pdf");
        }

        [Route("LogSil/{id}")]
        [HttpPost]
        public async Task<IActionResult> LogSil(int id)
        {
            try
            {
                string sql = "CvDownloadLogDelete";
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ID", id)
                };
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