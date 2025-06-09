using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminMailValidator : AbstractValidator<AdminMail>
    {
        public AdminMailValidator()
        {
            RuleFor(mail => mail.MailAdress)
                .MaximumLength(75).WithMessage("Mail adresi en fazla 75 karakter olabilir.")
                .MinimumLength(5).WithMessage("Mail adresi en az 5 karakter olmalıdır.")
                .EmailAddress().WithMessage("Geçerli bir mail adresi giriniz.");

            RuleFor(mail => mail.MailPassword)
                .MaximumLength(150).WithMessage("Mail şifresi en fazla 150 karakter olabilir.")
                .MinimumLength(3).WithMessage("Mail şifresi en az 3 karakter olmalıdır.");

            RuleFor(mail => mail.ServerName)
                .NotEmpty().WithMessage("Sunucu adı boş bırakılamaz.")
                .MaximumLength(20).WithMessage("Sunucu adı en fazla 20 karakter olabilir.")
                .MinimumLength(3).WithMessage("Sunucu adı en az 3 karakter olmalıdır.");

            RuleFor(mail => mail.MailPort)
                .InclusiveBetween(1, 65535).WithMessage("Mail portu 1 ile 65535 arasında olmalıdır.");

            RuleFor(mail => mail.IsSSL)
                .NotNull().WithMessage("SSL durumu belirtilmelidir.");
        }
    }
}