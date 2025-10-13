using FluentValidation;
using Microsoft.AspNetCore.Http;
using MyWebSite.Models;
using System.Text.RegularExpressions;

namespace MyWebSite.Business
{
    public class AdminCertificateValidator : AbstractValidator<CertificateUploadVm>
    {
        private static readonly Regex SafeText = new(@"^[a-zA-Z0-9ĞÜŞİÖÇğüşıöç\s\-_.,()]+$", RegexOptions.CultureInvariant);

        public AdminCertificateValidator()
        {
            RuleFor(x => x.SertifikaAdi)
          .NotEmpty().WithMessage("Sertifika adı zorunludur.")
          .MinimumLength(2).WithMessage("Sertifika adı en az 2 karakter olmalıdır.")
          .MaximumLength(100).WithMessage("Sertifika adı en fazla 100 karakter olabilir.");

            RuleFor(x => x.SertifikaAciklamasi)
                .MaximumLength(255).WithMessage("Açıklama 255 karakteri geçemez.");
            When(x => x.Id == 0, () =>
            {
                RuleFor(x => x.PdfDosyasi)
                    .NotNull().WithMessage("PDF dosyası seçilmedi.")
                    .Must(f => f == null || f.Length > 0)
                        .WithMessage("PDF boş olamaz.")
                    .Must(f => f == null || Path.GetExtension(f.FileName).ToLowerInvariant() == ".pdf")
                        .WithMessage("Sadece PDF dosyası kabul edilir.")
                    .Must(f => f == null || f.Length <= 15 * 1024 * 1024)
                        .WithMessage("PDF dosyası 15MB’tan büyük olamaz.");
            });

            When(x => x.Id > 0 && x.PdfDosyasi != null, () =>
            {
                RuleFor(x => x.PdfDosyasi)
                    .Must(f => f == null || f.Length > 0)
                        .WithMessage("PDF boş olamaz.")
                    .Must(f => f == null || Path.GetExtension(f.FileName).ToLowerInvariant() == ".pdf")
                        .WithMessage("Sadece PDF dosyası kabul edilir.")
                    .Must(f => f == null || f.Length <= 15 * 1024 * 1024)
                        .WithMessage("PDF dosyası 15MB’tan büyük olamaz.");
            });
        }
    }
    public class CertificateUploadVm
    {
        public int Id { get; set; }
        public string SertifikaAdi { get; set; } = string.Empty;
        public string? SertifikaAciklamasi { get; set; }
        public bool IsActive { get; set; }
        public IFormFile? PdfDosyasi { get; set; }
    }
}