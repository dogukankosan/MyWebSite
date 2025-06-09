using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminEducationValidator : AbstractValidator<Education>
    {
        public AdminEducationValidator()
        {
            RuleFor(education => education.SchoolName)
                .NotEmpty().WithMessage("Okul adı boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Okul adı en az 3 karakterden olmalıdır")
                .MaximumLength(75).WithMessage("Okul adı en fazla 75 karakter olabilir.");

            RuleFor(education => education.SectionName)
                .NotEmpty().WithMessage("Bölüm adı boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Bölüm adı en az 3 karakterden oluşmalıdır")
                .MaximumLength(75).WithMessage("Bölüm adı en fazla 75 karakter olabilir.");

            RuleFor(education => education.Years)
                .NotEmpty().WithMessage("Yıllar alanı boş bırakılamaz.")
                .Matches(@"^\d{4}-[A-Za-z0-9ÇçĞğİıÖöŞşÜü]+$").WithMessage("Yıl bilgisi sadece 2020-2025 veya 2020-AKTİF formatında olmalıdır.")
                .MaximumLength(12).WithMessage("Yıllar en fazla 12 karakter olabilir.");
        }
    }
}