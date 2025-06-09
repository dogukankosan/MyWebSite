using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Authentication;

[Route("Admin")]
[Authorize(Roles = "Admin")]
public class AdminHome : Controller
{
    [Route("Anasayfa")]
    public async Task<IActionResult> Index()
    {
        AdminDashboardViewModel model = new();
        try
        {
            List<SqlParameter> emptyParams = new();
            List<ReportResultModel> contactResults = await SQLCrud.ExecuteModelListAsync<ReportResultModel>(
                "ContactMonthReport",
                emptyParams,
                reader => new ReportResultModel
                {
                    Ay = Convert.ToByte(reader["Ay"]),
                    Sayisi = Convert.ToInt32(reader["Sayisi"])
                },
                CommandType.StoredProcedure
            );
            foreach (var item in contactResults)
                model.ContactMonthlyCounts[item.Ay - 1] = item.Sayisi;
            List<ReportResultModel> webLogResults = await SQLCrud.ExecuteModelListAsync<ReportResultModel>(
                "WebLogMonthReport",
                emptyParams,
                reader => new ReportResultModel
                {
                    Ay = Convert.ToByte(reader["Ay"]),
                    Sayisi = Convert.ToInt32(reader["Sayisi"])
                },
                CommandType.StoredProcedure
            );
            foreach (var item in webLogResults)
                model.WebLogMonthlyCounts[item.Ay - 1] = item.Sayisi;
            model.WebLogCount = await SQLCrud.ExecuteScalarAsync<int>("WebLogCount", emptyParams, 0, CommandType.StoredProcedure);
            model.ContactCount = await SQLCrud.ExecuteScalarAsync<int>("ContactCount", emptyParams, 0, CommandType.StoredProcedure);
            model.AdminLoginErrorCount = await SQLCrud.ExecuteScalarAsync<int>("AdminLoginErrorCount", emptyParams, 0, CommandType.StoredProcedure);
            model.ProjectCount = await SQLCrud.ExecuteScalarAsync<int>("ProjectCount", emptyParams, 0, CommandType.StoredProcedure);
            model.SkillsCount = await SQLCrud.ExecuteScalarAsync<int>("SkillsCount", emptyParams, 0, CommandType.StoredProcedure);
            model.AdminLogsCount = await SQLCrud.ExecuteScalarAsync<int>("AdminLogsCount", emptyParams, 0, CommandType.StoredProcedure);
            model.JobsCount = await SQLCrud.ExecuteScalarAsync<int>("JobsCount", emptyParams, 0, CommandType.StoredProcedure);
            model.EducationCount = await SQLCrud.ExecuteScalarAsync<int>("EducationCount", emptyParams, 0, CommandType.StoredProcedure);
            return View(model);
        }
        catch (Exception ex)
        {
            await Logging.LogAdd("Admin Rapor Ekranı Panelde Listeleme İşlemi Hatası", ex.Message);
            TempData["Type"] = "error";
            TempData["Message"] = "Admin Rapor Hatalı Listeleme İşlemi";
        }
        return View(model);
    }
    [Route("CikisYap")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}