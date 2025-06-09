namespace MyWebSite.Models
{
    public class About
    {
        public byte ID { get; set; }
        public IFormFile Picture1 { get; set; }
        public string AboutTitle { get; set; }
        public string AboutDetails1 { get; set; }
        public string AboutAdress { get; set; }
        public string AboutMail { get; set; }
        public string AboutPhone { get; set; }
        public string AboutWebSite { get; set; }
        public IFormFile Picture2 { get; set; } 
        public string AboutName { get; set; }
        public string AboutDetails2 { get; set; }
        public string IFrameAdress { get; set; }
        public string? ExistingPicture1 { get; set; }
        public string? ExistingPicture2 { get; set; }
    }
}