using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminEducationValidator : AbstractValidator<Education>
    {
        public AdminEducationValidator()
        {
            RuleFor(education => education.SchoolName)
                .NotEmpty().WithMessage("Okul adı boş bırakılamaz.").MinimumLength(3).WithMessage("Okul adı en az 3 karakterden olmalıdır")
                .MaximumLength(50).WithMessage("Okul adı en fazla 50 karakter olabilir.");
            RuleFor(education => education.SectionName)
                .NotEmpty().WithMessage("Bölüm adı boş bırakılamaz.").MinimumLength(3).WithMessage("Bölüm adı en az 3 karakterden oluşmalıdır")
                .MaximumLength(50).WithMessage("Bölüm adı en fazla 50 karakter olabilir.");
            RuleFor(education => education.Years)
                .NotEmpty().WithMessage("Yıllar alanı boş bırakılamaz.")
                .Matches(@"^\d{4}-\d{4}$").WithMessage("Yıllar alanı 4 basamaklı iki yıl arasında tire ile olmalıdır (örn. 2010-2014).")
                .MaximumLength(11).WithMessage("Yıllar en fazla 11 karakter olabilir.");
        }
    }
}