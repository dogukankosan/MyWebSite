using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [Route("AdminMail")]
    [Authorize(Roles = "Admin")]
    public class AdminMailController : Controller
    {
        [HttpGet]
        [Route("Liste")]
        public async Task<IActionResult> List()
        {
            try
            {
                List<AdminMail> mailList = await SQLCrud.ExecuteModelListAsync<AdminMail>(
                    "AdminMailGet",
                    null,
                    reader => new AdminMail
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        MailAdress = reader["MailAdress"].ToString(),
                        MailPassword = reader["MailPassword"].ToString(),
                        ServerName = reader["ServerName"].ToString(),
                        MailPort = Convert.ToInt32(reader["MailPort"]),
                        IsSSL = Convert.ToBoolean(reader["IsSSL"])
                    },
                    System.Data.CommandType.StoredProcedure
                );
                AdminMail model = mailList.FirstOrDefault() ?? new AdminMail();
                return View(model);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Mail Panelde Listeleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Mail Hatalı Listeleme İşlemi";
                return View(new AdminMail());
            }
        }
        [HttpGet]
        [Route("Guncelle/{id:int}")]
        public async Task<IActionResult> Update(int id)
        {
            return await List();
        }
        [HttpPost]
        [Route("Guncelle")]
        public async Task<IActionResult> Update(AdminMail adminMail)
        {
            if (!ModelState.IsValid)
            {
                Dictionary<string, string> errors = ModelState
                    .Where(e => e.Value.Errors.Any())
                    .ToDictionary(k => k.Key, v => string.Join(", ", v.Value.Errors.Select(e => e.ErrorMessage)));
                return Json(new { success = false, errors });
            }
            try
            {
                string finalPassword;
                if (string.IsNullOrWhiteSpace(adminMail.MailPassword))
                    finalPassword = adminMail.ExistingPassword;
                else
                    finalPassword = await HashingControl.Encrypt(adminMail.MailPassword);
                List<SqlParameter> parameters = new()
                {
                    new SqlParameter("@MailAdress", adminMail.MailAdress),
                    new SqlParameter("@MailPassword", finalPassword),
                    new SqlParameter("@ServerName", adminMail.ServerName),
                    new SqlParameter("@MailPort", adminMail.MailPort),
                    new SqlParameter("@IsSSL", adminMail.IsSSL)
                };
                await SQLCrud.InsertUpdateDeleteAsync("AdminMailUpdate", parameters, System.Data.CommandType.StoredProcedure);
                TempData["Type"] = "success";
                TempData["Message"] = "Admin Mail Başarılı Güncelleme İşlemi";
                return Json(new { success = true, redirectUrl = Url.Action("Liste", "AdminMail") });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Mail Panelde Güncelleme Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Admin Mail Hatalı Güncelleme İşlemi";
                return Json(new { success = false });
            }
        }
        [HttpGet]
        [Route("TestMail")]
        public async Task<IActionResult> TestMail()
        {
            try
            {
                MailSender sender = new();
                string htmlBody = @"
        <div style='font-family:Segoe UI,Arial,sans-serif; background-color:#f4f7fb; padding:30px;'>
            <div style='max-width:600px; margin:auto; background-color:#ffffff; border-radius:10px; 
                        box-shadow:0 2px 8px rgba(0,0,0,0.1); overflow:hidden;'>
                <div style='background:linear-gradient(90deg,#007bff,#00bfff); color:#fff; 
                            text-align:center; padding:15px 0; font-size:20px; font-weight:bold;'>
                    🚀 Web Sitesi Test Maili
                </div>
                <div style='padding:25px; color:#333;'>
                    <p>Merhaba,</p>
                    <p>Bu mail <strong>web sitenizin mail gönderim sisteminin testi</strong> amacıyla gönderilmiştir.</p>
                    <p style='margin:15px 0;'>Her şey düzgün çalışıyor 🎉</p>
                    <hr style='border:none; border-top:1px solid #eee; margin:20px 0;' />
                    <p style='font-size:13px; color:#777;'>📅 Gönderim Zamanı: " + DateTime.Now.ToString("dd MMMM yyyy HH:mm", new System.Globalization.CultureInfo("tr-TR")) + @"</p>
                </div>
                <div style='background-color:#f8f9fa; text-align:center; padding:10px; font-size:12px; color:#666;'>
                    © " + DateTime.Now.Year + @" Web Siteniz | Bu mail otomatik olarak gönderilmiştir.
                </div>
            </div>
        </div>";
                bool status = await sender.SendMail(
                    subject: "✅ Test Maili - Web Sitesi Başarılı",
                    body: htmlBody
                );
                if (status)
                {
                    TempData["Type"] = "success";
                    TempData["Message"] = "Test maili gönderme işlemi başarılı.";
                }
                else
                    throw new Exception("Mail gönderimi başarısız döndü.");
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Mail Panelde Test Mail Hatası", ex.Message);
                TempData["Type"] = "error";
                TempData["Message"] = "Test maili gönderilemedi.";
            }
            return RedirectToAction("Liste", "AdminMail");
        }
    }
}