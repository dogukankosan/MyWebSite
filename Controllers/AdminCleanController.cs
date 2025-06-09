using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
namespace MyWebSite.Controllers
{
    [Route("AdminTemizlik")]
    [Authorize(Roles = "Admin")]
    public class AdminCleanController : Controller
    {
        [Route("Temizlik")]
        [HttpGet]
        public IActionResult Clean()
        {
            return View();
        }
        [Route("TemizlikYap")]
        [HttpPost]
        public async Task<IActionResult> CleanUp()
        {
            try
            {
                bool temizlendi = await SQLCrud.InsertUpdateDeleteAsync("CleanDatabase",null, System.Data.CommandType.StoredProcedure);
                bool tablolarSilindi = await SQLCrud.InsertUpdateDeleteAsync(@"
                    TRUNCATE TABLE AdminLogs;
                    TRUNCATE TABLE AdminLoginError;
                    TRUNCATE TABLE WebLog;", null, System.Data.CommandType.Text);
                if (temizlendi && tablolarSilindi)
                {
                    TempData["FromCleanup"] = "True";
                    TempData["Type"] = "success";
                    TempData["Message"] = "Veritabanı temizleme işlemi başarıyla tamamlandı.";
                }
                else
                {
                    TempData["FromCleanup"] = "True";
                    TempData["Type"] = "error";
                    TempData["Message"] = "Temizlik sırasında bir hata oluştu. Lütfen tekrar deneyin.";
                }
            }
            catch (Exception ex)
            {
                TempData["FromCleanup"] = "True";
                TempData["Type"] = "error";
                TempData["Message"] = "Hata oluştu: " + ex.Message;
            }
            return RedirectToAction("Temizlik","AdminTemizlik");
        }
    }
}