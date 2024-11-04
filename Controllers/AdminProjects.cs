using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminProje")]
    [Authorize(Roles = "Admin")]
    public class AdminProjects : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminProjects(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("Liste")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            List<Projects> projects = new();
            string base64Image1 = "";
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("ProjectsGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (!Convert.IsDBNull(reader["ProjectImg"]))
                                {
                                    byte[] imageBytes = (byte[])reader["ProjectImg"];
                                    base64Image1 = Convert.ToBase64String(imageBytes);
                                }
                                projects.Add(new Projects()
                                {
                                    ID = Convert.ToByte(reader["ID"]),
                                    ProjectName = reader["ProjectName"].ToString(),
                                    Base64Pictures = base64Image1,
                                    ProjectDescription = reader["ProjectDescription"].ToString(),
                                    ProjectGithubLink = reader["ProjectGithubLink"].ToString(),
                                    ProjectLink = reader["ProjectLink"].ToString()
                                });
                            }
                            return View(projects);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Projeler Panelde Listeleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Projeler Hatalı Listeleme İşlemi";
                }
            }
            return View(projects);
        }
        [Route("Guncelle/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            string base64Image1 = "";
            Projects cs = new();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("ProjectsGetByID", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                cs.ID = Convert.ToByte(reader["ID"]);
                                cs.ProjectName = reader["ProjectName"].ToString();
                                cs.ProjectDescription = reader["ProjectDescription"].ToString();
                                cs.ProjectGithubLink = reader["ProjectGithubLink"].ToString();
                                cs.ProjectLink = reader["ProjectLink"].ToString();
                                if (!Convert.IsDBNull(reader["ProjectImg"]))
                                {
                                    byte[] imageBytes = (byte[])reader["ProjectImg"];
                                    base64Image1 = Convert.ToBase64String(imageBytes);
                                }
                            }
                            ViewData["picture"] = $"data:image/jpeg;base64,{base64Image1}";
                            return View(cs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Projeler Panelde Güncelle Listeleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Projeler Hatalı Güncelle Listeleme İşlemi";
                }
            }
            return View(cs);
        }
        [Route("Guncelle")]
        [HttpPost]
        public async Task<IActionResult> Update(Projects projects)
        {  
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("ProjectName"))
                    errors["ProjectName"] = string.Join(", ", ModelState["ProjectName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("ProjectDescription"))
                    errors["ProjectDescription"] = string.Join(", ", ModelState["ProjectDescription"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("ProjectGithubLink"))
                    errors["ProjectGithubLink"] = string.Join(", ", ModelState["ProjectGithubLink"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("ProjectImg"))
                    errors["ProjectImg"] = string.Join(", ", ModelState["ProjectImg"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            byte[] imageBytes1 = null;
            if (projects.ProjectImg != null && projects.ProjectImg.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await projects.ProjectImg.CopyToAsync(ms);
                    imageBytes1 = ms.ToArray();
                }
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("ProjectsUpdate", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", projects.ID);
                    cmd.Parameters.AddWithValue("@ProjectName", projects.ProjectName);
                    cmd.Parameters.AddWithValue("@ProjectDescription", projects.ProjectDescription);
                    cmd.Parameters.Add("@ProjectImg", SqlDbType.VarBinary).Value = (object)imageBytes1 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@ProjectGithubLink", projects.ProjectGithubLink);
                    cmd.Parameters.AddWithValue("@ProjectLink", projects.ProjectLink);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Projeler Başarılı Güncelleme İşlemi";
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Projeler Panelde Güncelleme Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Projeler Hatalı Güncelleme İşlemi";
                }
            }
            return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminProje") });
        }
        [HttpGet]
        [Route("Ekle")]
        public async Task<IActionResult> Add()
        {
            return View();
        }
        [HttpPost]
        [Route("Ekle")]
        public async Task<IActionResult> Add(Projects projects)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = new();
                if (ModelState.ContainsKey("ProjectName"))
                    errors["ProjectName"] = string.Join(", ", ModelState["ProjectName"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("ProjectDescription"))
                    errors["ProjectDescription"] = string.Join(", ", ModelState["ProjectDescription"].Errors.Select(e => e.ErrorMessage));
                if (ModelState.ContainsKey("ProjectGithubLink"))
                    errors["ProjectGithubLink"] = string.Join(", ", ModelState["ProjectGithubLink"].Errors.Select(e => e.ErrorMessage));
                return Json(new { success = false, errors });
            }
            byte[] imageBytes1 = null;
            if (projects.ProjectImg != null && projects.ProjectImg.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await projects.ProjectImg.CopyToAsync(ms);
                    imageBytes1 = ms.ToArray();
                }
            }
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("ProjectsInsert", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ProjectName", projects.ProjectName);
                    cmd.Parameters.AddWithValue("@ProjectDescription", projects.ProjectDescription);
                    cmd.Parameters.Add("@ProjectImg", SqlDbType.VarBinary).Value = (object)imageBytes1 ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@ProjectGithubLink", projects.ProjectGithubLink);
                    cmd.Parameters.AddWithValue("@ProjectLink", projects.ProjectLink);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Projeler Başarılı Ekleme İşlemi";
                    return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminProje") });
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Projeler Panelde Ekleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Projeler Hatalı Ekleme İşlemi";
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
                    SqlCommand cmd = new("ProjectsDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ID", id);
                    await cmd.ExecuteNonQueryAsync();
                    TempData["Type"] = "success";
                    TempData["Message"] = "Admin Projeler Başarılı Silme İşlemi";
                    return RedirectToAction("Liste", "AdminProje");
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Projeler Panelde Silme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Projeler Hatalı Silme İşlemi";
                }
                return View();
            }
        }
    }
}