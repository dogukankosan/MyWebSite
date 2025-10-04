namespace MyWebSite.Models
{
    public class CvDownloadLog
    {
        public int ID { get; set; }
        public string? IPAdress { get; set; }
        public string? UserGeo { get; set; }
        public string? UserInfo { get; set; }
        public DateTime DownloadDate { get; set; }
    }
}