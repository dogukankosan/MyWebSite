using FluentValidation;
using MyWebSite.Models;

namespace MyWebSite.Business
{
    public class AdminJobsValidator : AbstractValidator<Jobs>
    {
        public AdminJobsValidator()
        {
            RuleFor(job => job.JobName)
                .NotEmpty().WithMessage("İş adı boş bırakılamaz.").MinimumLength(3).WithMessage("İş adı 3 karakterden az olamaz")
                .MaximumLength(50).WithMessage("İş adı en fazla 50 karakter olabilir.");
            RuleFor(job => job.JobTitle)
                .NotEmpty().WithMessage("İş ünvanı boş bırakılamaz.").MinimumLength(3).WithMessage("İş ünvanı 3 karakterden az olamaz")
                .MaximumLength(50).WithMessage("İş ünvanı en fazla 50 karakter olabilir.");
            RuleFor(job => job.JobYears)
                .NotEmpty().WithMessage("İş yılları boş bırakılamaz.")
                .Matches(@"^\d{4}-\d{4}$").WithMessage("İş yılları 4 basamaklı iki yıl arasında tire ile olmalıdır (örn. 2010-2014).")
                .MaximumLength(20).WithMessage("İş yılları en fazla 20 karakter olabilir.");
            RuleFor(job => job.JobAbout)
                .NotEmpty().WithMessage("İş hakkında bilgi boş bırakılamaz.");
        }
    }
}