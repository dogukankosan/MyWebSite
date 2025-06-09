using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminWebLog")]
    [Authorize(Roles = "Admin")]
    public class AdminLog : Controller
    {
        [HttpGet("Liste")]
        public async Task<IActionResult> List()
        {
            try
            {
                List<AdminLogs> logList = await SQLCrud.ExecuteModelListAsync<AdminLogs>(
                    "WebLogGet",
                    null,
                    reader => new AdminLogs
                    {
                        ID = Convert.ToInt16(reader["ID"]),
                        IPAdress = reader["IPAdress"].ToString(),
                        UserGeo = reader["UserGeo"].ToString(),
                        UserInfo = reader["UserInfo"].ToString(),
                        CreateDate = Convert.ToDateTime(reader["CreateDate"])
                    },
                    System.Data.CommandType.StoredProcedure
                );
                return View(logList);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Siteye Giren Kullanıcı Panelde Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Listeleme işlemi sırasında bir hata oluştu.";
                return View(new List<AdminLogs>());
            }
        }
        [HttpGet("Sil/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ID", id)
                };
                await SQLCrud.InsertUpdateDeleteAsync("WebLogDelete", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Kayıt silme işlemi başarılı.";
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Siteye Giren Kullanıcı Panelde Silme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Silme işlemi sırasında bir hata oluştu.";
            }
            return RedirectToAction("Liste", "AdminWebLog");
        }
    }
}