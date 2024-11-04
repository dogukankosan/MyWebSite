using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminLoginPanelValidator : AbstractValidator<Login>
    {
        public AdminLoginPanelValidator()
        {
            RuleFor(admin => admin.UserName)
                .NotEmpty().WithMessage("Kullanıcı adı boş bırakılamaz.")
                .MaximumLength(50).WithMessage("Kullanıcı adı en fazla 50 karakter olabilir.")
                .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır.");
            RuleFor(admin => admin.Password)
         .NotEmpty().WithMessage("Şifre boş bırakılamaz.")
         .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.").MaximumLength(50).WithMessage("Şifre en fazla 50 karakterden oluşmalıdır");
        }
    }
}