using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminIletisim")]
    [Authorize(Roles = "Admin")]
    public class AdminContact : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminContact(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("Liste")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("ContactGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<Contacts> contacts = new();
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                contacts.Add(new Contacts()
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
                                });
                            }
                            return View(contacts);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin İletişim Panelde Listeleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin İletişim Hatalı Listeleme İşlemi";
                }
            }
            return View();
        }
        [HttpGet]
        [Route("Sil/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("ContactDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin İletişim Başarılı Silme İşlemi";
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin İletişim Panelde Silme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin İletişim Hatalı Silme İşlemi";
                }
            }
            return RedirectToAction("Liste", "AdminIletisim");
        }
    }
}