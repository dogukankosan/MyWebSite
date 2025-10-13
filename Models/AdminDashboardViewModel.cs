namespace MyWebSite.Models
{
    public class AdminDashboardViewModel
    {
        public int ContactCount { get; set; }
        public int WebLogCount { get; set; }
        public int AdminLoginErrorCount { get; set; }
        public int ProjectCount { get; set; }
        public int SkillsCount { get; set; }
        public int AdminLogsCount { get; set; }
        public int EducationCount { get; set; }
        public int FileDownloadCount { get; set; }
        public int CvDownloadCount { get; set; }
        public int CertificatesCount { get; set; }
        public int CertificateDownloadCount { get; set; }
        public int JobsCount { get; set; }
        public int[] ContactMonthlyCounts { get; set; } = new int[12];
        public int[] WebLogMonthlyCounts { get; set; } = new int[12];
    }
}