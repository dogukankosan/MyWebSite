using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminIcon")] 
    [Authorize(Roles = "Admin")]
    public class AdminIconsController : Controller
    {
        private static bool IsUniqueViolation(Exception ex)
        {
            Exception? cur = ex;
            while (cur != null)
            {
                if (cur is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                    return true;
                cur = cur.InnerException;
            }
            return false;
        }
        [HttpGet("Liste")]
        public async Task<IActionResult> List()
        {
            try
            {
                var icons = await SQLCrud.ExecuteModelListAsync(
                    "IconsGetAll",
                    new List<SqlParameter>(),
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
            icon.Icon = icon.Icon?.Trim();
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        k => k.Key.Contains('.') ? k.Key.Split('.').Last() : k.Key,
                        v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage))
                    );
                return Json(new { success = false, errors });
            }
            int exists = await SQLCrud.ExecuteScalarAsync<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.Icons WHERE Icon = @p) THEN 1 ELSE 0 END",
                new List<SqlParameter> { new("@p", icon.Icon ?? (object)DBNull.Value) },
                0,
                CommandType.Text
            );
            if (exists == 1)
            {
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string>
                    {
                        ["Icon"] = "Bu ikon zaten kayıtlı."
                    }
                });
            }
            bool ok = await SQLCrud.InsertUpdateDeleteAsync(
                "dbo.IconsInsert",
                new List<SqlParameter> { new("@IconsString", icon.Icon ?? (object)DBNull.Value) },
                CommandType.StoredProcedure
            );
            if (!ok)
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string>
                    {
                        ["_"] = "Kayıt eklenemedi."
                    }
                });
            TempData["Type"] = "success";
            TempData["Message"] = "İkon başarıyla eklendi.";
            return Json(new { success = true, redirectUrl = Url.Action("List", "AdminIcons") });
        }

        [HttpGet("Guncelle/{id:int}")]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter> { new SqlParameter("@ID", id) };
                var icons = await SQLCrud.ExecuteModelListAsync(
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
            icon.Icon = icon.Icon?.Trim();
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Where(x => x.Value.Errors.Any())
                    .ToDictionary(
                        k => k.Key.Contains('.') ? k.Key.Split('.').Last() : k.Key,
                        v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage))
                    );
                return Json(new { success = false, errors });
            }
            int existsOther = await SQLCrud.ExecuteScalarAsync<int>(
                @"SELECT CASE WHEN EXISTS (
              SELECT 1 FROM dbo.Icons WITH (NOLOCK)
              WHERE Icon = @p AND ID <> @id
          ) THEN 1 ELSE 0 END",
                new List<SqlParameter>
                {
            new("@p", icon.Icon ?? (object)DBNull.Value),
            new("@id", icon.ID)
                },
                0,
                CommandType.Text
            );
            if (existsOther == 1)
            {
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string>
                    {
                        ["Icon"] = "Bu ikon zaten başka kayıtta var."
                    }
                });
            }
            bool ok = await SQLCrud.InsertUpdateDeleteAsync(
                "dbo.IconsUpdate",
                new List<SqlParameter>
                {
            new("@ID", icon.ID),
            new("@IconsString", icon.Icon ?? (object)DBNull.Value)
                },
                CommandType.StoredProcedure
            );
            if (!ok)
            {
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<string, string>
                    {
                        ["_"] = "Güncelleme başarısız."
                    }
                });
            }
            TempData["Type"] = "success";
            TempData["Message"] = "İkon başarıyla güncellendi.";
            return Json(new { success = true, redirectUrl = Url.Action("List", "AdminIcons") });
        }
        [HttpGet("Sil/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter> { new SqlParameter("@ID", id) };
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