using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminIconsValidator : AbstractValidator<Icons>
    {
        public AdminIconsValidator()
        {
            RuleFor(x => x.Icon)
                .NotEmpty().WithMessage("İkon alanı boş bırakılamaz.")
                .MinimumLength(4).WithMessage("İkon en az 4 karakter olmalıdır.")
                .MaximumLength(200).WithMessage("İkon en fazla 200 karakter olabilir.");
        }
    }
}