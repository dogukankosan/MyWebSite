using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminBeceriler")]
    [Authorize(Roles = "Admin")]
    public class AdminSkills : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminSkills(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("Liste")]
        public async Task<IActionResult> List()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("SkillsGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<Skills> skills = new();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                skills.Add(new Skills()
                                {
                                    ID = Convert.ToByte(reader["ID"]),
                                    SkillName = reader["SkillName"].ToString(),
                                    SkillPercent = Convert.ToByte(reader["SkillPercent"]),
                                    Skillcon = reader["Skillcon"].ToString(),
                                });
                            }
                            return View(skills);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Beceriler Panelde Listeleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Beceriler Hatalı Listeleme İşlemi";
                }
            }
            return View();
        }
        [HttpGet]
        [Route("Ekle")]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        [Route("Ekle")]
        public async Task<IActionResult> Ekle(Skills skills)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("SkillName"))
                    errors["SkillName"] = string.Join(", ", ModelState["SkillName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("SkillPercent"))
                    errors["SkillPercent"] = string.Join(", ", ModelState["SkillPercent"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("Skillcon"))
                    errors["Skillcon"] = string.Join(", ", ModelState["Skillcon"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("SkillsAdd", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SkillName", skills.SkillName);
                    cmd.Parameters.AddWithValue("@SkillPercent", skills.SkillPercent);
                    cmd.Parameters.AddWithValue("@Skillcon", skills.Skillcon);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Beceri Başarılı Ekleme İşlemi";
                    return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminBeceriler") });
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Beceriler Panelde Ekleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Beceri Hatalı Ekleme İşlemi";
                    return Json(new { success = false, message = "Admin Beceri Hatalı Ekleme İşlemi" });
                }
            }
        }
        [Route("Sil/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("SkillsDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Beceriler Başarılı Silme İşlemi";
                    return RedirectToAction("Liste", "AdminBeceriler");
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Beceriler Panelde Silme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Beceriler Hatalı Silme İşlemi";
                }
                return View();
            }
        }
        [HttpGet]
        [Route("Guncelle/{id:int}")]
        public async Task<IActionResult> Update(int id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("SkillsGetByID", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    Skills skills = new();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                skills.ID = Convert.ToByte(reader["ID"]);
                                skills.SkillName = reader["SkillName"].ToString();
                                skills.SkillPercent = Convert.ToByte(reader["SkillPercent"]);
                                skills.Skillcon = reader["Skillcon"].ToString();
                            }
                            return View(skills);
                        }
                    }
                    return RedirectToAction("Liste", "AdminBeceriler");
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Beceriler Panelde Güncelleme Listesi İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Beceriler Hatalı Güncelleme Listesi İşlemi";
                }
            }
            return View();
        }
        [HttpPost]
        [Route("Guncelle")]
        public async Task<IActionResult> Update(Skills skills)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("SkillName"))
                    errors["SkillName"] = string.Join(", ", ModelState["SkillName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("SkillPercent"))
                    errors["SkillPercent"] = string.Join(", ", ModelState["SkillPercent"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("Skillcon"))
                    errors["Skillcon"] = string.Join(", ", ModelState["Skillcon"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("SkillsUpdate", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SkillName", skills.SkillName);
                    cmd.Parameters.AddWithValue("@SkillPercent", skills.SkillPercent);
                    cmd.Parameters.AddWithValue("@Skillcon", skills.Skillcon);
                    cmd.Parameters.AddWithValue("@ID", skills.ID);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Beceriler Güncelleme İşlemi";
                    return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminBeceriler") });
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Beceriler Panelde Güncelleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Beceriler Hatalı Güncelleme İşlemi";
                }
            }
            return View();
        }
    }
}