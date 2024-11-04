namespace MyWebSite.Models
{
    public class Contacts
    {
        public short ID { get; set; }
        public string ContactName { get; set; }
        public string ContactMail { get; set; }
        public string ContactPhone { get; set; }
        public string ContactSubject { get; set; }
        public string ContactMessage { get; set; }
        public DateTime CreateDate { get; set; }
        public string IPAdress { get; set; }
        public string UserGeo { get; set; }
        public string UserInfo { get; set; }
    }
}