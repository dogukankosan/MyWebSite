using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminHataLog")]
    [Authorize(Roles = "Admin")]
    public class AdminErrorLogController : Controller
    {
        [HttpGet("Liste")]
        public async Task<IActionResult> List()
        {
            try
            {
                List<AdminErrorLog> logList = await SQLCrud.ExecuteModelListAsync<AdminErrorLog>(
                    "AdminLogsGet",
                    null,
                    reader => new AdminErrorLog
                    {
                        ID = Convert.ToInt16(reader["ID"]),
                        LogType = reader["LogType"].ToString(),
                        ErrorMessage = reader["ErrorMessage"].ToString(),
                        IPAdress = reader["IPAdress"].ToString(),
                        CreateDate = Convert.ToDateTime(reader["CreateDate"])
                    },
                    System.Data.CommandType.StoredProcedure
                );
                return View(logList);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Hatalar Log Kayıtları Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Listeleme işlemi sırasında hata oluştu.";
                return View(new List<AdminErrorLog>());
            }
        }
        [HttpGet("Sil/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@ID", id)
                };
                await SQLCrud.InsertUpdateDeleteAsync("AdminLogsDelete", parameters, System.Data.CommandType.StoredProcedure);
                TempData["Type"] = "success";
                TempData["Message"] = "Log kaydı başarıyla silindi.";
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Hatalar Log Kayıtları Silme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Silme işlemi sırasında hata oluştu.";
            }
            return RedirectToAction("Liste", "AdminHataLog");
        }
    }
}