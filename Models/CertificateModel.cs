namespace MyWebSite.Models
{
    public class CertificateModel
    {
        public int Id { get; set; }
        public string CertificateName { get; set; } = string.Empty;
        public string? CertificateDescription { get; set; }
        public DateTime UploadDate { get; set; }
        public string? ThumbnailBase64 { get; set; }
        public bool IsActive { get; set; }
    }
}