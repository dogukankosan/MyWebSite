using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminHome : Controller
    {
        private readonly IConfiguration _configuration;
        public AdminHome(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [Route("Anasayfa")]
        public async Task<IActionResult> Index()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    int[] count = new int[12];
                    int[] count2 = new int[12];
                    byte step = 0;
                    await con.OpenAsync();
                    SqlCommand cmd = new("ContactMonthReport", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                step = byte.Parse(reader["Ay"].ToString());
                                count[step - 1] = int.Parse(reader["Sayisi"].ToString());
                            }
                        }
                    }
                    SqlCommand cmd1 = new("WebLogMonthReport", con);
                    cmd1.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader1 = await cmd1.ExecuteReaderAsync())
                    {
                        if (reader1.HasRows)
                        {
                            while (await reader1.ReadAsync())
                            {
                                step = byte.Parse(reader1["Ay"].ToString());
                                count2[step - 1] = int.Parse(reader1["Sayisi"].ToString());
                            }
                        }
                    }
                    SqlCommand cmd2 = new("WebLogCount", con);
                    cmd2.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader2 = await cmd2.ExecuteReaderAsync())
                    {
                        if (reader2.HasRows)
                        {
                            while (await reader2.ReadAsync())
                            {
                                ViewBag.WebLogCount = int.Parse(reader2["Sayisi"].ToString());
                            }
                        }
                    }
                    SqlCommand cmd3 = new("ContactCount", con);
                    cmd3.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader3 = await cmd3.ExecuteReaderAsync())
                    {
                        if (reader3.HasRows)
                        {
                            while (await reader3.ReadAsync())
                            {
                                ViewBag.ContactCount = int.Parse(reader3["Sayisi"].ToString());
                            }
                        }
                    }
                    SqlCommand cmd4 = new("AdminLoginErrorCount", con);
                    cmd4.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader4 = await cmd4.ExecuteReaderAsync())
                    {
                        if (reader4.HasRows)
                        {
                            while (await reader4.ReadAsync())
                            {
                                ViewBag.AdminLoginErrorCount = int.Parse(reader4["Sayisi"].ToString());
                            }
                        }
                    }
                    ViewBag.MonthlyCounts = count;
                    ViewBag.MonthlyWebLog = count2;
                    return View();
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Admin Rapor Ekranı Panelde Listeleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                    TempData["Type"] = "error";
                    TempData["Message"] = "Admin Rapor Hatalı Listeleme İşlemi";
                }
            }
            return View();
        }
        [Route("CikisYap")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}