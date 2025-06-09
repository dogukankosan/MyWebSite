using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminJobsValidator : AbstractValidator<Jobs>
    {
        public AdminJobsValidator()
        {
            RuleFor(job => job.JobName)
                .NotEmpty().WithMessage("İş adı boş bırakılamaz.")
                .MinimumLength(3).WithMessage("İş adı 3 karakterden az olamaz.")
                .MaximumLength(75).WithMessage("İş adı en fazla 75 karakter olabilir.");

            RuleFor(job => job.JobTitle)
                .NotEmpty().WithMessage("İş unvanı boş bırakılamaz.")
                .MinimumLength(3).WithMessage("İş unvanı 3 karakterden az olamaz.")
                .MaximumLength(75).WithMessage("İş unvanı en fazla 75 karakter olabilir.");

            RuleFor(job => job.JobYears)
                .NotEmpty().WithMessage("Yıllar alanı boş bırakılamaz.")
                .Matches(@"^\d{4}-[A-Za-z0-9ÇçĞğİıÖöŞşÜü]+$").WithMessage("Yıl bilgisi sadece 2020-2025 veya 2020-AKTİF formatında olmalıdır.")
                .MaximumLength(12).WithMessage("Yıllar en fazla 12 karakter olabilir.");

            RuleFor(job => job.JobAbout)
                .NotEmpty().WithMessage("İş hakkında bilgi boş bırakılamaz.");
        }
    }
}