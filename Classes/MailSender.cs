using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace MyWebSite.Classes
{
    public class MailSender
    {
        private string _connectionString;
        public MailSender(string configuration)
        {
            _connectionString = configuration;
        }
        public async Task SendMail(string subject, string body, string ip = "")
        {
            Models.AdminMail cs = new();
            string connectionString = _connectionString;
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("AdminMailGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                cs.ID = Convert.ToByte(reader["ID"]);
                                cs.MailAdress = reader["MailAdress"].ToString();
                                cs.MailPassword = reader["MailPassword"].ToString();
                                cs.ServerName = reader["ServerName"].ToString();
                                cs.MailPort = Convert.ToInt32(reader["MailPort"]);
                                cs.IsSSL = Convert.ToBoolean(reader["IsSSL"]);
                            }
                            if (string.IsNullOrEmpty(cs.MailAdress))
                                throw new Exception();
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Mail Bilgileri Girilmemiş Veya Okunamamıştır", ex.Message, connectionString, ip);
                    return;
                }
            }
            try
            {
                MailMessage mail = new();
                mail.From = new(cs.MailAdress);
                mail.To.Add(cs.MailAdress);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = false;
                SmtpClient smtpClient = new(cs.ServerName, cs.MailPort);
                smtpClient.Credentials = new NetworkCredential(cs.MailAdress, cs.MailPassword);
                smtpClient.EnableSsl = cs.IsSSL;
                await smtpClient.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Mail Gönderilemedi", ex.Message, connectionString, ip);
                return;
            }
        }
    }
}