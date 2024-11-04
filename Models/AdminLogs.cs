namespace MyWebSite.Models
{
    public class AdminLogs
    {
        public int ID { get; set; }
        public string IPAdress { get; set; }
        public string UserGeo { get; set; }
        public string UserInfo { get; set; }
        public DateTime CreateDate { get; set; }
    }
}