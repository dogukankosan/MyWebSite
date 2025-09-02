using FluentValidation;
using MyWebSite.Models;
using System.Text.RegularExpressions;

namespace MyWebSite.Business
{
    public class FileUploadVmValidator : AbstractValidator<FileUploadVm>

    {
        private static readonly Regex VersionStrict = new(@"^\d+\.\d+\.\d+$", RegexOptions.CultureInvariant);
        private static readonly Regex AllowedSegment = new(@"^[A-Za-z0-9._-]{1,64}$", RegexOptions.CultureInvariant);
        public FileUploadVmValidator()
        {
            RuleFor(x => x.PackageKey)
                .NotEmpty().WithMessage("Paket zorunludur.")
                .Must(v => AllowedSegment.IsMatch(v ?? "")).WithMessage("Paket anahtarı yalnızca harf/rakam . _ - içerebilir (max 64).");

            RuleFor(x => x.Version)
                .NotEmpty().WithMessage("Versiyon zorunludur.")
                .Must(v => VersionStrict.IsMatch(v ?? "")).WithMessage("Versiyon biçimi 1.2.3 olmalı.");

            RuleFor(x => x.DownloadPasswordPlain)
                .NotEmpty().WithMessage("İndirme şifresi zorunludur.")
                .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalı.");

            RuleFor(x => x.Archive)
                .NotNull().WithMessage("Dosya seçilmedi.")
                .Must(f => f!.Length > 0).WithMessage("Dosya boş olamaz.")
                .Must(f =>
                {
                    string? ext = System.IO.Path.GetExtension(f!.FileName).ToLowerInvariant();
                    return ext == ".zip" || ext == ".rar";
                }).WithMessage("Sadece .zip veya .rar kabul edilir.")
                .Must(f => f!.Length <= 1L * 1024 * 1024 * 1024)
                .WithMessage("Dosya boyutu 1GB’ı geçemez.");
        }
    }
}