namespace MyWebSite.Models
{
    public class CertificateDownloadLogModel
    {
        public int Id { get; set; }
        public int CertificateId { get; set; }
        public string CertificateName { get; set; } = string.Empty;
        public DateTime DownloadUtc { get; set; }
        public string ClientIp { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? UserAgent { get; set; }
    }
}