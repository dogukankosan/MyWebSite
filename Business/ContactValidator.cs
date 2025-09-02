using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class ContactValidator : AbstractValidator<Contacts>
    {
        public ContactValidator()
        {
            RuleFor(user => user.ContactName)
                .NotEmpty().WithMessage("İsminiz boş geçilemez.")
                .Length(2, 75).WithMessage("İsim 2 ile 75 karakter arasında olmalıdır.")
                .Matches(@"^[a-zA-ZğüşöçİĞÜŞÖÇ\s]+$").WithMessage("İsim yalnızca harflerden oluşmalıdır.");

            RuleFor(mail => mail.ContactMail)
                .NotEmpty().WithMessage("E-posta boş geçilemez.")
                .Length(5, 75).WithMessage("E-posta 5 ile 75 karakter arasında olmalıdır.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("Geçerli bir alan adı giriniz.").WithMessage("E-posta adresi '.com' içermelidir.");

            RuleFor(x => x.ContactPhone)
            .NotEmpty().WithMessage("Telefon numarası boş geçilemez.")
            .MinimumLength(7).WithMessage("Telefon numarası en az 7 karakter olmalıdır.")
            .MaximumLength(25).WithMessage("Telefon numarası en fazla 25 karakter olabilir.")
            .Matches(@"^\+?[0-9\s\-\(\)]{7,25}$")
                .WithMessage("Geçerli bir telefon numarası giriniz. (+90 532 123 4567 gibi)");

            RuleFor(user => user.ContactSubject)
                .NotEmpty().WithMessage("Konu boş geçilemez.")
                .Length(4, 75).WithMessage("Konu 4 ile 75 karakter arasında olmalıdır.");

            RuleFor(user => user.ContactMessage)
                .NotEmpty().WithMessage("Mesaj boş geçilemez.")
                .Length(4, 500).WithMessage("Mesaj 4 ile 500 karakter arasında olmalıdır.");
        }
    }
}