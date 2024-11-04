using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class ContactValidator : AbstractValidator<Contacts>
    {
        public ContactValidator()
        {
            RuleFor(user => user.ContactName)
        .NotEmpty().WithMessage("İsminiz Boş Geçilemez.")
        .Length(2, 40).WithMessage("İsminiz 2 ile 40 Karakter Uzunluğunda Olmalıdır.")
        .Matches(@"^[a-zA-ZğüşöçİĞÜŞÖÇ\s]+$").WithMessage("İsminiz yalnızca harflerden oluşmalıdır. Sayılar ve özel karakterler kullanılamaz.");
            RuleFor(mail => mail.ContactMail)
    .NotEmpty().WithMessage("Mailiniz Boş Geçilemez.")
    .EmailAddress().WithMessage("Geçerli Bir E-Posta Adresi Giriniz.").Length(4, 40).WithMessage("İsminiz 4 ile 40 Karakter Uzunluğunda Olmalıdır.")
    .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$").WithMessage("E-Posta Adresi '@' İçermeli Ve Geçerli Bir Alan Adı Olmalıdır.")
    .Matches(@".+\.com").WithMessage("E-posta adresi .com içermelidir.");
            RuleFor(user => user.ContactPhone).NotEmpty().WithMessage("Telefon Numaranız Boş Geçilemez.")
    .Matches(@"^\d{10}$").WithMessage("Telefon Numaranız 10 Rakamdan Oluşmalı Ve Sayısal Olmalıdır.")
    .Length(10).WithMessage("Telefon Numarası Tam Olarak 10 Karakter Olmalıdır.");
            RuleFor(user => user.ContactSubject)
      .NotEmpty().WithMessage("Konu Boş Geçilemez.")
      .MinimumLength(4).WithMessage("Konu En Az 4 Karakter Olmalıdır.")
      .MaximumLength(50).WithMessage("Konu En Fazla 50 Karakter Olabilir.");
            RuleFor(user => user.ContactMessage).NotEmpty().WithMessage("Mesaj Boş Geçilemez.").MinimumLength(4).WithMessage("Mesaj En Az 4 Karakter Olmalıdır.")
      .MaximumLength(500).WithMessage("Mesaj En Fazla 500 Karakter Olabilir.");
        }
    }
}