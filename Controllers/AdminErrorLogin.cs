using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminHataliGiris")]
    [Authorize(Roles = "Admin")]
    public class AdminErrorLoginController : Controller
    {
        [HttpGet("Liste")]
        public async Task<IActionResult> List()
        {
            try
            {
                List<AdminErrorLogin> errorList = await SQLCrud.ExecuteModelListAsync<AdminErrorLogin>(
                    "AdminLoginErrorGetAll",
                    null,
                    reader => new AdminErrorLogin
                    {
                        ID = Convert.ToInt16(reader["ID"]),
                        IPAdress = reader["IPAdress"].ToString(),
                        Geo = reader["Geo"].ToString(),
                        UserInfo = reader["UserInfo"].ToString(),
                        CreateDate = Convert.ToDateTime(reader["CreateDate"])
                    },
                    System.Data.CommandType.StoredProcedure
                );
                return View(errorList);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Hatalı Giriş Bilgileri Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Listeleme işlemi sırasında hata oluştu.";
                return View(new List<AdminErrorLogin>());
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

                await SQLCrud.InsertUpdateDeleteAsync("AdminLoginErrorDelete", parameters, System.Data.CommandType.StoredProcedure);

                TempData["Type"] = "success";
                TempData["Message"] = "Kayıt başarıyla silindi.";
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Hatalı Giriş Silme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Silme işlemi sırasında hata oluştu.";
            }
            return RedirectToAction("Liste", "AdminHataliGiris");
        }
    }
}