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
                .MaximumLength(25).WithMessage("Yetenek adı en fazla 25 karakter olabilir.").MinimumLength(3).WithMessage("Yetenek adı en az 3 karakterden oluşmalıdır"); ;
            RuleFor(skill => skill.SkillPercent)
                .NotEmpty().WithMessage("Yetenek yüzdesi boş bırakılamaz.")
                .Must(x => x > 0 && x <= 100).WithMessage("Yetenek yüzdesi 0 ile 100 arasında olmalıdır.");
            RuleFor(skill => skill.Skillcon)
                .NotEmpty().WithMessage("Yetenek ikonu boş bırakılamaz.")
                .MaximumLength(50).WithMessage("Yetenek ikonu en fazla 50 karakter olabilir.").MinimumLength(5).WithMessage("Yetenek ikonu en az 5 karakterden oluşmalıdır");
        }
    }
}