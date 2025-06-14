using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminProjectsValidator : AbstractValidator<Projects>
    {
        public AdminProjectsValidator()
        {
            RuleFor(project => project.ProjectName)
                .NotEmpty().WithMessage("Proje adı boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Proje adı en az 3 karakter olabilir.")
                .MaximumLength(75).WithMessage("Proje adı en fazla 75 karakter olabilir.");

            RuleFor(project => project.ProjectDescription)
                .NotEmpty().WithMessage("Proje açıklaması boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Proje açıklaması en az 3 karakter olabilir.");

            RuleFor(project => project.ProjectGithubLink)
                .NotEmpty().WithMessage("GitHub linki boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Github linki en az 3 karakter olabilir.")
                .MaximumLength(75).WithMessage("GitHub linki en fazla 75 karakter olabilir.")
                .Must(link => Uri.IsWellFormedUriString(link, UriKind.Absolute))
                .WithMessage("Geçerli bir GitHub URL'si giriniz.");

            RuleFor(project => project.ProjectLink)
                .NotEmpty().WithMessage("Proje linki boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Proje linki en az 3 karakter olabilir.")
                .MaximumLength(75).WithMessage("Proje linki en fazla 75 karakter olabilir.")
                .Must(link => Uri.IsWellFormedUriString(link, UriKind.Absolute))
                .WithMessage("Geçerli bir proje linki giriniz.");
            RuleFor(x => x.ProjectImg)
                .Must((model, file) =>
                {
                    if (file != null && file.Length > 0)
                        return true;
                    return !string.IsNullOrEmpty(model.Base64Pictures);
                })
                .WithMessage("En az bir görsel yüklenmelidir.");
        }
    }
}