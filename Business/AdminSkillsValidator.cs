using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminSkillsValidator : AbstractValidator<Skills>
    {
        public AdminSkillsValidator()
        {
            RuleFor(skill => skill.SkillName)
                .NotEmpty().WithMessage("Yetenek adı boş bırakılamaz.")
                .MaximumLength(50).WithMessage("Yetenek adı en fazla 50 karakter olabilir.")
                .MinimumLength(3).WithMessage("Yetenek adı en az 3 karakter olmalıdır.");

            RuleFor(skill => skill.SkillPercent)
                .Must(x => x >= 1 && x <= 100)
                .WithMessage("Yetenek yüzdesi 1 ile 100 arasında olmalıdır.");

            RuleFor(skill => skill.Skillcon)
                .NotEmpty().WithMessage("Yetenek ikonu boş bırakılamaz.")
                .MaximumLength(75).WithMessage("Yetenek ikonu en fazla 75 karakter olabilir.")
                .MinimumLength(5).WithMessage("Yetenek ikonu en az 5 karakter olmalıdır.");
        }
    }
}