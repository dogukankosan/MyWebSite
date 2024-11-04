namespace MyWebSite.Models
{
    public class AdminErrorLog
    {
        public int ID { get; set; }
        public string LogType { get; set; }
        public string ErrorMessage { get; set; }
        public string IPAdress { get; set; }
        public DateTime CreateDate { get; set; }
    }
}