namespace MyWebSite.Models
{
    public class AdminErrorLogin
    {
        public int ID { get; set; }
        public string IPAdress { get; set; }
        public string Geo { get; set; }
        public string UserInfo { get; set; }
        public DateTime CreateDate { get; set; }
    }
}