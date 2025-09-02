namespace MyWebSite.Models
{
    public sealed class FileListItem
    {
        public int Id { get; set; }
        public string PackageKey { get; set; }
        public string Version { get; set; }
        public string ArchiveType { get; set; }
        public string FileNameOriginal { get; set; }
        public string FileNameStored { get; set; }
        public string RelativePath { get; set; }
        public long FileSizeBytes { get; set; }
        public string Sha256 { get; set; }
        public DateTime UploadUtc { get; set; }
        public DateTime LastModifiedUtc { get; set; }
        public bool IsActive { get; set; }
        public int DownloadCount { get; set; }
    }
    public sealed class FileDetails
    {
        public int Id { get; set; }
        public string PackageKey { get; set; }
        public string Version { get; set; }
        public string ArchiveType { get; set; }
        public string FileNameOriginal { get; set; }
        public string FileNameStored { get; set; }
        public string RelativePath { get; set; }
        public long FileSizeBytes { get; set; }
        public string Sha256 { get; set; }
        public DateTime UploadUtc { get; set; }
        public DateTime LastModifiedUtc { get; set; }
        public bool IsActive { get; set; }
        public int DownloadCount { get; set; }
        public string DownloadPasswordHash { get; set; }
    }
    public sealed class FileUploadVm
    {
        public string PackageKey { get; set; }
        public string Version { get; set; }
        public IFormFile Archive { get; set; }
        public string DownloadPasswordPlain { get; set; }
    }
    public sealed class DownloadPromptVm
    {
        public int Id { get; set; }
        public string PackageKey { get; set; }
        public string Version { get; set; }
        public string FileNameStored { get; set; }
        public string ArchiveType { get; set; }
    }
}