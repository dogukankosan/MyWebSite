using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        var result = await SQLCrud.ExecuteModelListAsync<Login>(
            "AdminLoginGet",
            new List<SqlParameter>(),
            r => new Login
            {
                UserName = r["AdminUserName"].ToString(),
                Password = r["Password_"].ToString()
            },
            CommandType.StoredProcedure
        );
        return View(result.FirstOrDefault() ?? new Login());
    }
    [HttpGet("Guncelle")]
    public async Task<IActionResult> Update(bool? logoutAfter = null)
    {
        var result = await SQLCrud.ExecuteModelListAsync<AdminLoginProp>(
            "AdminLoginGet",
            null,
            r => new AdminLoginProp
            {
                UserName = r["AdminUserName"].ToString(),
                ExistingPasswordHash = r["Password_"].ToString()
            },
            CommandType.StoredProcedure
        );
        ViewBag.LogoutAfter = logoutAfter == true;
        ViewBag.LogoutUrl = Url.Action("Cikis", "AdminUserLogin",
            new { returnUrl = Url.Action("Panel", "AdminGiris") });

        return View(result.FirstOrDefault() ?? new AdminLoginProp());
    }
    [HttpPost("Guncelle")]
    public async Task<IActionResult> Update(AdminLoginProp login)
    {
        if (!ModelState.IsValid) return View(login);

        try
        {
            string finalPassword = string.IsNullOrWhiteSpace(login.Password)
                ? login.ExistingPasswordHash
                : HashingControl.HashPassword(login.Password);
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new("@AdminUserName", login.UserName),
                new("@Password_", finalPassword)
            };
            await SQLCrud.InsertUpdateDeleteAsync("AdminLoginUpdate", parameters, CommandType.StoredProcedure);
            TempData["Type"] = "success";
            TempData["Message"] = "Güncelleme başarılı. 5 saniye içinde yeniden giriş ekranına yönlendirileceksiniz.";
            return RedirectToAction("Guncelle", "AdminKullanici", new { logoutAfter = true });
        }
        catch (Exception ex)
        {
            await Logging.LogAdd("Admin Giriş Güncelleme Hatası", ex.Message);
            ModelState.AddModelError("", "Güncelleme sırasında beklenmeyen bir hata oluştu.");
            return View(login);
        }
    }
    [HttpGet("Cikis")]
    [AllowAnonymous] 
    public async Task<IActionResult> Cikis(string? returnUrl = null)
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect(returnUrl ?? Url.Action("Panel", "AdminGiris"));
    }
}