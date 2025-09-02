namespace MyWebSite.Models
{
    public class DownloadLogItem
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string PackageKey { get; set; } = "";
        public string Version { get; set; } = "";
        public string FileNameStored { get; set; } = "";
        public DateTime DownloadUtc { get; set; }
        public string ClientIp { get; set; } = "";
        public string? UserAgent { get; set; }
        public int DownloadIndex { get; set; }
        public string? City { get; set; }  
    }
}