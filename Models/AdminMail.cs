namespace MyWebSite.Models
{
    public class AdminMail
    {
        public byte ID { get; set; }
        public string MailAdress { get; set; }
        public string MailPassword { get; set; }
        public string ServerName { get; set; }
        public int MailPort { get; set; }
        public bool IsSSL { get; set; }
    }
}