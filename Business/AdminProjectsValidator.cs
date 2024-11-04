using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminProjectsValidator : AbstractValidator<Projects>
    {
        public AdminProjectsValidator()
        {
            RuleFor(project => project.ProjectName)
                .NotEmpty().WithMessage("Proje adı boş bırakılamaz.").MinimumLength(3).WithMessage("Proje adı en az 3 karakter olabilir")
                .MaximumLength(50).WithMessage("Proje adı en fazla 50 karakter olabilir.");
            RuleFor(project => project.ProjectDescription)
                .NotEmpty().WithMessage("Proje açıklaması boş bırakılamaz.").MinimumLength(3).WithMessage("Proje açıklaması en az 3 karakter olabilir")
                .MaximumLength(300).WithMessage("Proje açıklaması en fazla 300 karakter olabilir.");
            RuleFor(project => project.ProjectGithubLink)
                .NotEmpty().WithMessage("GitHub linki boş bırakılamaz.").MinimumLength(3).WithMessage("Github linki en az 3 karakter olabilir")
                .MaximumLength(50).WithMessage("GitHub linki en fazla 50 karakter olabilir.")
                .Must(link => Uri.IsWellFormedUriString(link, UriKind.Absolute))
                .WithMessage("Geçerli bir URL giriniz.");
        }
    }
}