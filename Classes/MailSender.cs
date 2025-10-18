using MyWebSite.Models;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace MyWebSite.Classes
{
    internal class MailSender
    {
        public AdminMail CustomSettings { get; set; }

        internal async Task<bool> SendMail(string subject, string body)
        {
            try
            {
                AdminMail mailSettings;
                if (CustomSettings != null)
                {
                    mailSettings = CustomSettings;
                }
                else
                {
                    List<SqlParameter> emptyParams = new();
                    List<AdminMail> adminMailList = await SQLCrud.ExecuteModelListAsync("AdminMailGet", emptyParams, reader => new AdminMail
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        MailAdress = reader["MailAdress"].ToString(),
                        MailPassword = reader["MailPassword"].ToString(),
                        ServerName = reader["ServerName"].ToString(),
                        MailPort = Convert.ToInt32(reader["MailPort"]),
                        IsSSL = Convert.ToBoolean(reader["IsSSL"])
                    });
                    mailSettings = adminMailList.FirstOrDefault();
                }
                if (mailSettings == null || string.IsNullOrEmpty(mailSettings.MailAdress))
                    throw new Exception("Mail ayarları eksik veya okunamadı.");
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress(mailSettings.MailAdress),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(mailSettings.MailAdress);
                using SmtpClient smtpClient = new SmtpClient(mailSettings.ServerName, mailSettings.MailPort)
                {
                    Credentials = new NetworkCredential(
                        mailSettings.MailAdress,
                        CustomSettings != null
                            ? mailSettings.MailPassword
                            : await HashingControl.Decrypt(mailSettings.MailPassword)
                    ),
                    EnableSsl = mailSettings.IsSSL
                };
                await smtpClient.SendMailAsync(mail);
                return true;
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Mail Gönderilemedi", ex.Message);
                return false;
            }
        }
    }
}