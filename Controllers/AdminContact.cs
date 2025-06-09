using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminIletisim")]
    [Authorize(Roles = "Admin")]
    public class AdminContact : Controller
    {
        [Route("Liste")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                List<Contacts> contactList = await SQLCrud.ExecuteModelListAsync("ContactGet", null, delegate (SqlDataReader reader)
                {
                    Contacts model = new Contacts
                    {
                        ID = Convert.ToInt16(reader["ID"]),
                        ContactName = reader["ContactName"].ToString(),
                        ContactMail = reader["ContactMail"].ToString(),
                        ContactPhone = reader["ContactPhone"].ToString(),
                        ContactSubject = reader["ContactSubject"].ToString(),
                        ContactMessage = reader["ContactMessage"].ToString(),
                        CreateDate = Convert.ToDateTime(reader["CreateDate"]),
                        IPAdress = reader["IPAdress"].ToString(),
                        UserGeo = reader["UserGeo"].ToString(),
                        UserInfo = reader["UserInfo"].ToString()
                    };
                    return model;
                });
                return View(contactList);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin İletişim Panelde Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin İletişim Hatalı Listeleme İşlemi";
                return View(new List<Contacts>());
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
                await SQLCrud.InsertUpdateDeleteAsync("ContactDelete", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Admin İletişim Başarılı Silme İşlemi";
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin İletişim Panelde Silme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin İletişim Hatalı Silme İşlemi";
            }
            return RedirectToAction("Liste", "AdminIletisim");
        }
    }
}