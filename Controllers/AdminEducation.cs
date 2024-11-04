using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminEgitim")]
    [Authorize(Roles = "Admin")]
    public class AdminEducation : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminEducation(IConfiguration configuration)
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
                    SqlCommand cmd = new("EducationGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<Education> educations = new();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                educations.Add(new Education()
                                {
                                    ID = Convert.ToByte(reader["ID"]),
                                    SchoolName = reader["SchoolName"].ToString(),
                                    SectionName = reader["SectionName"].ToString(),
                                    Years = reader["Years"].ToString(),
                                });
                            }
                            return View(educations);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Eğitim Panelde Listeleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Eğitim Hatalı Listeleme İşlemi";
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
        public async Task<IActionResult> Add(Education education)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("SchoolName"))
                    errors["SchoolName"] = string.Join(", ", ModelState["SchoolName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("SectionName"))
                    errors["SectionName"] = string.Join(", ", ModelState["SectionName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("Years"))
                    errors["Years"] = string.Join(", ", ModelState["Years"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("EducationAdd", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SchoolName", education.SchoolName);
                    cmd.Parameters.AddWithValue("@SectionName", education.SectionName);
                    cmd.Parameters.AddWithValue("@Years", education.Years);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Eğitim Başarılı Ekleme İşlemi";
                    return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminEgitim") });
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Eğitim Panelde Ekleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Eğitim Hatalı Ekleme İşlemi";
                }
            }
            return View();
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
                    SqlCommand cmd = new("EducationDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Eğitim Başarılı Silme İşlemi";
                    return RedirectToAction("Liste", "AdminEgitim");
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Eğitim Panelde Silme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Eğitim Hatalı Silme İşlemi";
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
                    SqlCommand cmd = new("EducationGetByID", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    Education education = new();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                education.ID = Convert.ToByte(reader["ID"]);
                                education.SchoolName = reader["SchoolName"].ToString();
                                education.SectionName = reader["SectionName"].ToString();
                                education.Years = reader["Years"].ToString();
                            }
                            return View(education);
                        }
                    }
                    return RedirectToAction("Liste", "AdminEgitim");
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Eğitim Panelde Güncelleme Listesi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Eğitim Hatalı Güncelleme Listesi İşlemi";
                }
            }
            return View();
        }
        [HttpPost]
        [Route("Guncelle")]
        public async Task<IActionResult> Update(Education education)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("SchoolName"))
                    errors["SchoolName"] = string.Join(", ", ModelState["SchoolName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("SectionName"))
                    errors["SectionName"] = string.Join(", ", ModelState["SectionName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("Years"))
                    errors["Years"] = string.Join(", ", ModelState["Years"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("EducationUpdate", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", education.ID);
                    cmd.Parameters.AddWithValue("@SchoolName", education.SchoolName);
                    cmd.Parameters.AddWithValue("@SectionName", education.SectionName);
                    cmd.Parameters.AddWithValue("@Years", education.Years);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Eğitim Başarılı Güncelleme İşlemi";
                    return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminEgitim") });
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Eğitim Panelde Güncelleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Eğitim Hatalı Güncelleme İşlemi";
                }
            }
            return View();
        }
    }
}