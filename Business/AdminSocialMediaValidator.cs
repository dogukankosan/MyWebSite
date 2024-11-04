using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminSocialMediaValidator : AbstractValidator<SocialMedia>
    {
        public AdminSocialMediaValidator()
        {
            RuleFor(social => social.FacebookLink)
                .NotEmpty().WithMessage("Facebook linki boş bırakılamaz.")
                .MaximumLength(200).WithMessage("Facebook linki en fazla 200 karakter olabilir.").MinimumLength(5).WithMessage("Facebook linki en az 5 karakterden oluşmalıdır.");
            RuleFor(social => social.GithubLink)
                .NotEmpty().WithMessage("Github linki boş bırakılamaz.")
                .MaximumLength(200).WithMessage("Github linki en fazla 200 karakter olabilir.").MinimumLength(5).WithMessage("Github linki en az 5 karakterden oluşmalıdır.");
            RuleFor(social => social.InstagramLink)
                .NotEmpty().WithMessage("Instagram linki boş bırakılamaz.")
                .MaximumLength(200).WithMessage("Instagram linki en fazla 200 karakter olabilir.").MinimumLength(5).WithMessage("Instagram linki en az 5 karakterden oluşmalıdır.");
            RuleFor(social => social.LinkedinLink)
                .NotEmpty().WithMessage("Linkedin linki boş bırakılamaz.")
                .MaximumLength(200).WithMessage("Linkedin linki en fazla 200 karakter olabilir.").MinimumLength(5).WithMessage("Linkedin linki en az 5 karakterden oluşmalıdır.");
        }
    }
}