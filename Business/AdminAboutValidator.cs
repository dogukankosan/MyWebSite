using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminAboutValidator : AbstractValidator<About>
    {
        public AdminAboutValidator()
        {
            RuleFor(about => about.AboutTitle)
                .NotEmpty().WithMessage("Ünvan boş geçilemez")
                .MinimumLength(2).WithMessage("Ünvan en az 2 karakter olmalıdır")
                .MaximumLength(50).WithMessage("Ünvan en fazla 50 karakter olmalıdır")
                .Matches(@"^[^\d]*$").WithMessage("Ünvan rakam içeremez");
            RuleFor(about => about.AboutDetails1)
                .NotEmpty().WithMessage("Detaylar 1 alanı boş bırakılamaz.").MinimumLength(5).WithMessage("Detaylar 1 en az 5 karakterden oluşmalıdır");
            RuleFor(about => about.AboutAdress)
                .NotEmpty().WithMessage("Adres alanı boş bırakılamaz.").MinimumLength(5).WithMessage("Adres alanı en az 5 karakterden oluşmalıdır")
                .MaximumLength(30).WithMessage("Adres en fazla 30 karakter olmalıdır.");
            RuleFor(about => about.AboutMail)
      .NotEmpty().WithMessage("E-posta boş bırakılamaz.")
      .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
      .MaximumLength(40).WithMessage("E-posta en fazla 40 karakter olmalıdır.")
       .Matches(@".+\.com").WithMessage("E-posta adresi .com içermelidir.");
            RuleFor(about => about.AboutPhone)
                .NotEmpty().WithMessage("Telefon numarası boş bırakılamaz.")
                .Matches(@"^[0-9\+\-\(\)\s]*$").WithMessage("Telefon numarası yalnızca rakamlar, boşluk, +, -, (, ) sembollerinden oluşabilir.")
                .MaximumLength(15).WithMessage("Telefon numarası en fazla 15 karakter olmalıdır.");
            RuleFor(about => about.AboutWebSite)
                .NotEmpty().WithMessage("Web sitesi boş bırakılamaz.").
                MinimumLength(4).WithMessage("Web sitesi en az 4 karakterden oluşmalıdır")
                .MaximumLength(30).WithMessage("Web sitesi en fazla 30 karakter olmalıdır.");
            RuleFor(about => about.AboutName)
                .NotEmpty().WithMessage("İsim boş bırakılamaz.")
                .MinimumLength(2).WithMessage("İsim en az 2 karakter olmalıdır.")
                .MaximumLength(30).WithMessage("İsim en fazla 30 karakter olmalıdır.")
                .Matches(@"^[^\d]*$").WithMessage("İsim rakam içeremez.");
            RuleFor(about => about.AboutDetails2)
                .NotEmpty().WithMessage("Detaylar 2 alanı boş bırakılamaz.").MinimumLength(5).WithMessage("Detaylar 2 en az 5 karakterden oluşmalıdır");
            RuleFor(about => about.IFrameAdress).NotEmpty().WithMessage("Harita boş geçilemez")
                .MaximumLength(int.MaxValue).WithMessage("Harita adresi çok uzun olamaz.").MinimumLength(50).WithMessage("Harita en az 50 karakterden oluşmalıdır");
        }
    }
}