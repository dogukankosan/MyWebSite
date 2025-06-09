using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminCv")]
    [Authorize(Roles = "Admin")]
    public class AdminCVController : Controller
    {
        [Route("Cv")]
        [HttpGet]
        public IActionResult Cv()
        {
            return View();
        }
        [Route("Guncelle")]
        [HttpPost]
        public async Task<IActionResult> CvGuncelle(IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                TempData["Message"] = "Lütfen bir dosya seçin.";
                TempData["Type"] = "error";
                return RedirectToAction("Cv");
            }
            if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
            {
                TempData["Message"] = "Sadece .pdf uzantılı dosyalar yüklenebilir.";
                TempData["Type"] = "error";
                return RedirectToAction("Cv");
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
            return RedirectToAction("Cv");
        }
    }
}