using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

[Route("AdminKullanici")]
[Authorize(Roles = "Admin")]
public class AdminUserLogin : Controller
{
    [HttpGet("Liste")]
    public async Task<IActionResult> Liste(string type = null, string message = null)
    {
        ViewBag.Type = type;
        ViewBag.Message = message;

        List<Login> result = await SQLCrud.ExecuteModelListAsync<Login>(
            "AdminLoginGet",
            new List<SqlParameter>(),
            reader => new Login
            {
                UserName = reader["AdminUserName"].ToString(),
                Password = reader["Password_"].ToString()
            },
            CommandType.StoredProcedure
        );
        return View(result.FirstOrDefault() ?? new Login());
    }
    [HttpGet("Guncelle")]
    public async Task<IActionResult> Update()
    {
        List<AdminLoginProp> result = await SQLCrud.ExecuteModelListAsync<AdminLoginProp>(
            "AdminLoginGet",
            null,
            reader => new AdminLoginProp
            {
                UserName = reader["AdminUserName"].ToString(),
                ExistingPasswordHash = reader["Password_"].ToString()
            },
            CommandType.StoredProcedure
        );
        return View(result.FirstOrDefault() ?? new AdminLoginProp());
    }
    [HttpPost("Guncelle")]
    public async Task<IActionResult> Update(AdminLoginProp login)
    {
        if (!ModelState.IsValid)
            return View(login);
        try
        {
            string finalPassword = string.IsNullOrWhiteSpace(login.Password)
                ? login.ExistingPasswordHash
                : HashingControl.HashPassword(login.Password);
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@AdminUserName", login.UserName),
                new SqlParameter("@Password_", finalPassword)
            };
            await SQLCrud.InsertUpdateDeleteAsync("AdminLoginUpdate", parameters, CommandType.StoredProcedure);
            TempData["Type"] = "success";
            TempData["Message"] = "Admin Giriş Bilgileri Başarıyla Güncellendi";
            return RedirectToAction("Liste");
        }
        catch (Exception ex)
        {
            await Logging.LogAdd("Admin Giriş Güncelleme Hatası", ex.Message);
            ModelState.AddModelError("", "Güncelleme sırasında beklenmeyen bir hata oluştu.");
            return View(login);
        }
    }
}