using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminHakkinda")]
    [Authorize(Roles = "Admin")]
    public class AdminAbout : Controller
    {
        [Route("Liste")]
        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                List<About> aboutList = await SQLCrud.ExecuteModelListAsync("AboutGet", null, delegate (SqlDataReader reader)
                {
                    SetPictureViewBags(reader);
                    return MapAbout(reader);
                });

                About model = aboutList.FirstOrDefault() ?? new About();
                return View(model);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Hakkında Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Listeleme sırasında hata oluştu.";
                return NotFound();
            }
        }
        [Route("Guncelle/{id:int}")]
        [HttpGet]
        public async Task<IActionResult> Update()
        {
            try
            {
                List<About> aboutList = await SQLCrud.ExecuteModelListAsync("AboutGet", null, delegate (SqlDataReader reader)
                {
                    SetPictureViewBags(reader);
                    return MapAbout(reader);
                });

                About model = aboutList.FirstOrDefault() ?? new About();
                return View(model);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Hakkında Güncelleme Listesi Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Güncelleme ekranı açılırken hata oluştu.";
                return View(new About());
            }
        }
        [Route("Guncelle")]
        [HttpPost]
        public async Task<IActionResult> Update(About about)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = ModelState
                    .Where(entry => entry.Value.Errors.Any())
                    .ToDictionary(
                        entry => entry.Key,
                        entry => string.Join(", ", entry.Value.Errors.Select(error => error.ErrorMessage))
                    );
                return Json(new { success = false, errors });
            }
            try
            {
                byte[]? imageBytes1 = await GetBytes(about.Picture1);
                byte[]? imageBytes2 = await GetBytes(about.Picture2);
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@Picture1", SqlDbType.VarBinary) { Value = (object?)imageBytes1 ?? DBNull.Value },
                    new SqlParameter("@Picture2", SqlDbType.VarBinary) { Value = (object?)imageBytes2 ?? DBNull.Value },
                    new SqlParameter("@AboutTitle", about.AboutTitle),
                    new SqlParameter("@AboutDetails1", about.AboutDetails1),
                    new SqlParameter("@AboutAdress", about.AboutAdress),
                    new SqlParameter("@AboutMail", about.AboutMail),
                    new SqlParameter("@AboutPhone", about.AboutPhone),
                    new SqlParameter("@AboutWebSite", about.AboutWebSite),
                    new SqlParameter("@AboutName", about.AboutName),
                    new SqlParameter("@AboutDetails2", about.AboutDetails2),
                    new SqlParameter("@IFrameAdress", about.IFrameAdress)
                };
                await SQLCrud.InsertUpdateDeleteAsync("AboutUpdate", parameters, CommandType.StoredProcedure);

                TempData["Type"] = "success";
                TempData["Message"] = "Başarıyla güncellendi.";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminHakkinda") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Hakkında Güncelleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Güncelleme sırasında hata oluştu.";
                return Json(new { success = false });
            }
        }
        private static About MapAbout(SqlDataReader reader)
        {
            About about = new About
            {
                ID = reader["ID"] != DBNull.Value ? Convert.ToByte(reader["ID"]) : default,
                AboutTitle = reader["AboutTitle"]?.ToString(),
                AboutDetails1 = reader["AboutDetails1"]?.ToString(),
                AboutAdress = reader["AboutAdress"]?.ToString(),
                AboutMail = reader["AboutMail"]?.ToString(),
                AboutPhone = reader["AboutPhone"]?.ToString(),
                AboutWebSite = reader["AboutWebSite"]?.ToString(),
                AboutName = reader["AboutName"]?.ToString(),
                AboutDetails2 = reader["AboutDetails2"]?.ToString(),
                IFrameAdress = reader["IFrameAdress"]?.ToString()
            };
            return about;
        }
        private void SetPictureViewBags(SqlDataReader reader)
        {
            if (!Convert.IsDBNull(reader["Picture1"]))
            {
                byte[] picture1 = (byte[])reader["Picture1"];
                ViewBag.Picture1 = $"data:image/jpeg;base64,{Convert.ToBase64String(picture1)}";
            }
            if (!Convert.IsDBNull(reader["Picture2"]))
            {
                byte[] picture2 = (byte[])reader["Picture2"];
                ViewBag.Picture2 = $"data:image/jpeg;base64,{Convert.ToBase64String(picture2)}";
            }
        }
        private static async Task<byte[]?> GetBytes(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;
            using MemoryStream memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}