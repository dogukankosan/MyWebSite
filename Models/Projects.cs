namespace MyWebSite.Models
{
    public class Projects
    {
        public byte ID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public IFormFile ProjectImg { get; set; }
        public string Base64Pictures { get; set; }
        public string ProjectGithubLink { get; set; }
        public string ProjectLink { get; set; }
    }
}