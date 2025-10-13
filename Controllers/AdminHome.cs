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
            var contactTask = SQLCrud.ExecuteModelListAsync<ReportResultModel>(
                "ContactMonthReport",
                emptyParams,
                reader => new ReportResultModel
                {
                    Ay = Convert.ToByte(reader["Ay"]),
                    Sayisi = Convert.ToInt32(reader["Sayisi"])
                },
                CommandType.StoredProcedure
            );
            var webLogTask = SQLCrud.ExecuteModelListAsync<ReportResultModel>(
                "WebLogMonthReport",
                emptyParams,
                reader => new ReportResultModel
                {
                    Ay = Convert.ToByte(reader["Ay"]),
                    Sayisi = Convert.ToInt32(reader["Sayisi"])
                },
                CommandType.StoredProcedure
            );
            var scalarTasks = new Dictionary<string, Task<int>>
            {
                ["WebLogCount"] = SQLCrud.ExecuteScalarAsync<int>("WebLogCount", emptyParams, 0, CommandType.StoredProcedure),
                ["ContactCount"] = SQLCrud.ExecuteScalarAsync<int>("ContactCount", emptyParams, 0, CommandType.StoredProcedure),
                ["AdminLoginErrorCount"] = SQLCrud.ExecuteScalarAsync<int>("AdminLoginErrorCount", emptyParams, 0, CommandType.StoredProcedure),
                ["ProjectCount"] = SQLCrud.ExecuteScalarAsync<int>("ProjectCount", emptyParams, 0, CommandType.StoredProcedure),
                ["SkillsCount"] = SQLCrud.ExecuteScalarAsync<int>("SkillsCount", emptyParams, 0, CommandType.StoredProcedure),
                ["AdminLogsCount"] = SQLCrud.ExecuteScalarAsync<int>("AdminLogsCount", emptyParams, 0, CommandType.StoredProcedure),
                ["JobsCount"] = SQLCrud.ExecuteScalarAsync<int>("JobsCount", emptyParams, 0, CommandType.StoredProcedure),
                ["EducationCount"] = SQLCrud.ExecuteScalarAsync<int>("EducationCount", emptyParams, 0, CommandType.StoredProcedure),
                ["FileDownloadCount"] = SQLCrud.ExecuteScalarAsync<int>("FileDownloadCount", emptyParams, 0, CommandType.StoredProcedure),
                ["CvDownloadCount"] = SQLCrud.ExecuteScalarAsync<int>("CvDownloadCount", emptyParams, 0, CommandType.StoredProcedure),
                ["CertificatesCount"] = SQLCrud.ExecuteScalarAsync<int>("CertificatesCount", emptyParams, 0, CommandType.StoredProcedure),
                ["CertificateDownloadCount"] = SQLCrud.ExecuteScalarAsync<int>("CertificateDownloadCount", emptyParams, 0, CommandType.StoredProcedure)
            };
            await Task.WhenAll(contactTask, webLogTask, Task.WhenAll(scalarTasks.Values));
            foreach (var item in contactTask.Result)
                if (item.Ay >= 1 && item.Ay <= 12)
                    model.ContactMonthlyCounts[item.Ay - 1] = item.Sayisi;
            foreach (var item in webLogTask.Result)
                if (item.Ay >= 1 && item.Ay <= 12)
                    model.WebLogMonthlyCounts[item.Ay - 1] = item.Sayisi;
            model.WebLogCount = scalarTasks["WebLogCount"].Result;
            model.ContactCount = scalarTasks["ContactCount"].Result;
            model.AdminLoginErrorCount = scalarTasks["AdminLoginErrorCount"].Result;
            model.ProjectCount = scalarTasks["ProjectCount"].Result;
            model.SkillsCount = scalarTasks["SkillsCount"].Result;
            model.AdminLogsCount = scalarTasks["AdminLogsCount"].Result;
            model.JobsCount = scalarTasks["JobsCount"].Result;
            model.EducationCount = scalarTasks["EducationCount"].Result;
            model.FileDownloadCount = scalarTasks["FileDownloadCount"].Result;
            model.CvDownloadCount = scalarTasks["CvDownloadCount"].Result;
            model.CertificatesCount = scalarTasks["CertificatesCount"].Result;
            model.CertificateDownloadCount = scalarTasks["CertificateDownloadCount"].Result;
            return View(model);
        }
        catch (Exception ex)
        {
            await Logging.LogAdd("Admin Rapor Ekranı Panelde Listeleme Hatası", $"{ex.Message}\n{ex.StackTrace}");
            TempData["Type"] = "error";
            TempData["Message"] = "Admin Rapor Hatalı Listeleme İşlemi";
            return View(model);
        }
    }
    [Route("CikisYap")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}