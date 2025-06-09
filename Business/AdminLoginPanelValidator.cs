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
                .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır.")
                .MaximumLength(50).WithMessage("Kullanıcı adı en fazla 50 karakter olabilir.")
                .Matches(@"^[a-zA-Z0-9_.-]+$").WithMessage("Kullanıcı adı sadece harf, rakam, nokta, alt çizgi ve tire içerebilir.");

            RuleFor(admin => admin.Password)
                .NotEmpty().WithMessage("Şifre boş bırakılamaz.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .MaximumLength(150).WithMessage("Şifre en fazla 150 karakter olabilir.")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
                .WithMessage("Şifre en az 1 büyük harf, 1 küçük harf, 1 rakam ve 1 özel karakter içermelidir.");
        }
    }
}