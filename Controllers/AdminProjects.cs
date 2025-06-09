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
        [HttpGet("Liste")]
        public async Task<IActionResult> List()
        {
            try
            {
                List<Projects> projects = await SQLCrud.ExecuteModelListAsync("ProjectsGet", null, reader =>
                {
                    string base64Image = string.Empty;
                    if (!Convert.IsDBNull(reader["ProjectImg"]))
                    {
                        byte[] imageBytes = (byte[])reader["ProjectImg"];
                        base64Image = Convert.ToBase64String(imageBytes);
                    }
                    return new Projects
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        ProjectName = reader["ProjectName"].ToString(),
                        Base64Pictures = base64Image,
                        ProjectDescription = reader["ProjectDescription"].ToString(),
                        ProjectGithubLink = reader["ProjectGithubLink"].ToString(),
                        ProjectLink = reader["ProjectLink"].ToString()
                    };
                });
                return View(projects);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Projeler Panelde Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Proje listeleme sırasında hata oluştu.";
                return View(new List<Projects>());
            }
        }
        [HttpGet("Guncelle/{id:int}")]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter> { new SqlParameter("@ID", id) };
                List<Projects> project = await SQLCrud.ExecuteModelListAsync("ProjectsGetByID", parameters, reader =>
                {
                    string base64Image = string.Empty;
                    if (!Convert.IsDBNull(reader["ProjectImg"]))
                    {
                        byte[] imageBytes = (byte[])reader["ProjectImg"];
                        base64Image = Convert.ToBase64String(imageBytes);
                    }
                    return new Projects
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        ProjectName = reader["ProjectName"].ToString(),
                        ProjectDescription = reader["ProjectDescription"].ToString(),
                        ProjectGithubLink = reader["ProjectGithubLink"].ToString(),
                        ProjectLink = reader["ProjectLink"].ToString(),
                        Base64Pictures = base64Image
                    };
                });
                Projects result = project.FirstOrDefault();
                if (result?.Base64Pictures != null)
                    ViewData["picture"] = "data:image/jpeg;base64," + result.Base64Pictures;
                return View(result ?? new Projects());
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Projeler Panelde Güncelleme Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Güncelleme sayfası yüklenemedi.";
                return View(new Projects());
            }
        }
        [HttpPost("Guncelle")]
        public async Task<IActionResult> Update(Projects projects)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(k => k.Key, v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage)));
                return Json(new { success = false, errors });
            }
            byte[] imageBytes = null;
            if (projects.ProjectImg != null && projects.ProjectImg.Length > 0)
            {
                await using MemoryStream ms = new MemoryStream();
                await projects.ProjectImg.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@ID", projects.ID),
                new SqlParameter("@ProjectName", projects.ProjectName),
                new SqlParameter("@ProjectDescription", projects.ProjectDescription),
                new SqlParameter("@ProjectImg", SqlDbType.VarBinary) { Value = (object)imageBytes ?? DBNull.Value },
                new SqlParameter("@ProjectGithubLink", projects.ProjectGithubLink),
                new SqlParameter("@ProjectLink", projects.ProjectLink)
            };
            try
            {
                await SQLCrud.InsertUpdateDeleteAsync("ProjectsUpdate", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Proje başarıyla güncellendi.";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminProje") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Projeler Güncelleme Hatası", ex.Message);
                return Json(new { success = false });
            }
        }
        [HttpGet("Ekle")]
        public IActionResult Add() => View();
        [HttpPost("Ekle")]
        public async Task<IActionResult> Add(Projects projects)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(k => k.Key, v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage)));
                return Json(new { success = false, errors });
            }
            byte[] imageBytes = null;
            if (projects.ProjectImg != null && projects.ProjectImg.Length > 0)
            {
                await using MemoryStream ms = new MemoryStream();
                await projects.ProjectImg.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@ProjectName", projects.ProjectName),
                new SqlParameter("@ProjectDescription", projects.ProjectDescription),
                new SqlParameter("@ProjectImg", SqlDbType.VarBinary) { Value = (object)imageBytes ?? DBNull.Value },
                new SqlParameter("@ProjectGithubLink", projects.ProjectGithubLink),
                new SqlParameter("@ProjectLink", projects.ProjectLink)
            };
            try
            {
                await SQLCrud.InsertUpdateDeleteAsync("ProjectsInsert", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Proje başarıyla eklendi.";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminProje") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Projeler Ekleme Hatası", ex.Message);
                return Json(new { success = false });
            }
        }
        [HttpGet("Sil/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            List<SqlParameter> parameters = new List<SqlParameter> { new SqlParameter("@ID", id) };
            try
            {
                await SQLCrud.InsertUpdateDeleteAsync("ProjectsDelete", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "Proje başarıyla silindi.";
                return RedirectToAction("Liste", "AdminProje");
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Projeler Silme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Proje silinirken hata oluştu.";
                return RedirectToAction("Liste", "AdminProje");
            }
        }
    }
}