using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminLoginValidator : AbstractValidator<AdminLoginProp>
    {
        public AdminLoginValidator()
        {
            RuleFor(admin => admin.UserName)
                .NotEmpty().WithMessage("Kullanıcı adı boş bırakılamaz.")
                .MaximumLength(50).WithMessage("Kullanıcı adı en fazla 50 karakter olabilir.")
                .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır.");
            RuleFor(admin => admin.Password)
                .NotEmpty().WithMessage("Şifre boş bırakılamaz.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .Matches(@"[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
                .Matches(@"[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
                .Matches(@"[0-9]").WithMessage("Şifre en az bir rakam içermelidir.")
                .Matches(@"[\W]").WithMessage("Şifre en az bir özel karakter içermelidir.");
        }
    }
}