using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminIsHayati")]
    [Authorize(Roles = "Admin")]
    public class AdminWorkLife : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminWorkLife(IConfiguration configuration)
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
                    SqlCommand cmd = new("JobsGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<Jobs> jobs = new();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                jobs.Add(new Jobs()
                                {
                                    ID = Convert.ToByte(reader["ID"]),
                                    JobName = reader["JobName"].ToString(),
                                    JobTitle = reader["JobTitle"].ToString(),
                                    JobYears = reader["JobYears"].ToString(),
                                    JobAbout = reader["JobAbout"].ToString(),
                                });
                            }
                            return View(jobs);
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
        [Route("Ekle")]
        public IActionResult Add()
        {
            return View();
        }
        [HttpPost]
        [Route("Ekle")]
        public async Task<IActionResult> Add(Jobs jobs)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("JobName"))
                    errors["JobName"] = string.Join(", ", ModelState["JobName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("JobTitle"))
                    errors["JobTitle"] = string.Join(", ", ModelState["JobTitle"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("JobYears"))
                    errors["JobYears"] = string.Join(", ", ModelState["JobYears"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("JobAbout"))
                    errors["JobAbout"] = string.Join(", ", ModelState["JobAbout"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("JobsAdd", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@JobName", jobs.JobName);
                    cmd.Parameters.AddWithValue("@JobTitle", jobs.JobTitle);
                    cmd.Parameters.AddWithValue("@JobYears", jobs.JobYears);
                    cmd.Parameters.AddWithValue("@JobAbout", jobs.JobAbout);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin İş Hayatı Başarılı Ekleme İşlemi";
                    return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminIsHayati") });
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
                    SqlCommand cmd = new("JobsDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin İş Hayatı Başarılı Silme İşlemi";
                    return RedirectToAction("Liste", "AdminIsHayati");
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
                    SqlCommand cmd = new("JobsGetByID", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    Jobs jobs = new();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                jobs.ID = Convert.ToByte(reader["ID"]);
                                jobs.JobName = reader["JobName"].ToString();
                                jobs.JobTitle = reader["JobTitle"].ToString();
                                jobs.JobYears = reader["JobYears"].ToString();
                                jobs.JobAbout = reader["JobAbout"].ToString();
                            }
                            return View(jobs);
                        }
                    }
                    return RedirectToAction("Liste", "AdminIsHayati");
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Eğitim Panelde Güncelleme Listesi İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Eğitim Güncelleme Listesi İşlemi Hatası";
                }
            }
            return View();
        }
        [HttpPost]
        [Route("Guncelle")]
        public async Task<IActionResult> Update(Jobs jobs)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("JobName"))
                    errors["JobName"] = string.Join(", ", ModelState["JobName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("JobTitle"))
                    errors["JobTitle"] = string.Join(", ", ModelState["JobTitle"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("JobYears"))
                    errors["JobYears"] = string.Join(", ", ModelState["JobYears"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("JobAbout"))
                    errors["JobAbout"] = string.Join(", ", ModelState["JobAbout"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("JobsUpdate", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", jobs.ID);
                    cmd.Parameters.AddWithValue("@JobName", jobs.JobName);
                    cmd.Parameters.AddWithValue("@JobTitle", jobs.JobTitle);
                    cmd.Parameters.AddWithValue("@JobYears", jobs.JobYears);
                    cmd.Parameters.AddWithValue("@JobAbout", jobs.JobAbout);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin İş Hayatı Başarılı Güncelleme İşlemi";
                    return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminIsHayati") });
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Eğitim Panelde Güncelleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Eğitim Güncelleme İşlemi Hatası";
                }
            }
            return View();
        }
    }
}