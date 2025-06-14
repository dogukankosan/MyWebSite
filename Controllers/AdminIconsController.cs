using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminIcon")]
    [Authorize(Roles = "Admin")]
    public class AdminIconsController : Controller
    {
        [HttpGet("Liste")]
        public async Task<IActionResult> List()
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                List<Icons> icons = await SQLCrud.ExecuteModelListAsync(
                    "IconsGetAll",
                    parameters,
                    reader => new Icons
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        Icon = reader["Icon"].ToString()
                    },
                    System.Data.CommandType.StoredProcedure
                );
                return View(icons);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Icon Panelde Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Icon listeleme işlemi sırasında hata oluştu.";
                return View();
            }
        }
        [HttpGet("Ekle")]
        public IActionResult Add() => View();

        [HttpPost("Ekle")]
        public async Task<IActionResult> Add(Icons icon)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        k => k.Key.Contains('.') ? k.Key.Split('.').Last() : k.Key,
                        v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage))
                    );
                return Json(new { success = false, errors });
            }
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@IconsString", icon.Icon ?? (object)DBNull.Value)
                };
                await SQLCrud.InsertUpdateDeleteAsync("IconsInsert", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "İkon başarıyla eklendi.";
                return Json(new { success = true, redirectUrl = Url.Action("List", "AdminIcons") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin İkon Panelde Ekleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Icon ekleme işlemi sırasında hata oluştu.";
                return Json(new { success = false });
            }
        }
        [HttpGet("Guncelle/{id:int}")]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@ID", id)
        };
                List<Icons> icons = await SQLCrud.ExecuteModelListAsync(
                    "IconsGetByID",
                    parameters,
                    reader => new Icons
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        Icon = reader["Icon"].ToString()
                    }
                );
                return View(icons.FirstOrDefault() ?? new Icons());
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Icon Panelde Güncelleme Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Icon güncelleme formu yüklenirken hata oluştu.";
                return View(new Icons());
            }
        }
        [HttpPost("Guncelle")]
        public async Task<IActionResult> Update(Icons icon)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        k => k.Key.Contains('.') ? k.Key.Split('.').Last() : k.Key,
                        v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage))
                    );
                return Json(new { success = false, errors });
            }
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
        {
            new SqlParameter("@ID", icon.ID),
            new SqlParameter("@IconsString", icon.Icon ?? (object)DBNull.Value)
        };
                await SQLCrud.InsertUpdateDeleteAsync("IconsUpdate", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "İkon başarıyla güncellendi.";
                return Json(new { success = true, redirectUrl = Url.Action("List", "AdminIcons") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Icon Panelde Güncelleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Icon güncelleme işlemi sırasında hata oluştu.";
                return Json(new { success = false });
            }
        }
        [HttpGet("Sil/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>
                {
                    new SqlParameter("@ID", id)
                };
                await SQLCrud.InsertUpdateDeleteAsync("IconDelete", parameters);
                TempData["Type"] = "success";
                TempData["Message"] = "İkon başarıyla silindi.";
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Icon Panelde Silme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "İkon silme işlemi sırasında hata oluştu.";
            }
            return RedirectToAction("List", "AdminIcons");
        }
    }
}