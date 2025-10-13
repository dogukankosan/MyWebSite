using FluentValidation;
using FluentValidation.Results;
using Ghostscript.NET;
using Ghostscript.NET.Rasterizer;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Business;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;

namespace MyWebSite.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("AdminSertifika")]
    public class AdminCertificatesController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _http;
        private readonly IValidator<CertificateUploadVm> _validator;
        private const string SP_GET_ALL = "dbo.CertificatesGet";
        private const string SP_INSERT = "dbo.CertificatesInsert";
        private const string SP_UPDATE = "dbo.CertificatesUpdate";
        private const string SP_UPDATE_ISACTIVE = "dbo.CertificatesToggle";
        private const string SP_DELETE = "dbo.CertificatesDelete";
        public AdminCertificatesController(
            IWebHostEnvironment env,
            IHttpContextAccessor http,
            IValidator<CertificateUploadVm> validator)
        {
            _env = env;
            _http = http;
            _validator = validator;
        }
        [HttpGet("")]
        [HttpGet("Liste")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var list = await SQLCrud.ExecuteModelListAsync<CertificateModel>(
                    SP_GET_ALL,
                    null,
                    reader => new CertificateModel
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        CertificateName = reader["CertificateName"].ToString()!,
                        CertificateDescription = reader["CertificateDescription"].ToString(),
                        UploadDate = Convert.ToDateTime(reader["UploadDate"]),
                        IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                        ThumbnailBase64 = reader["CertificateThumbnail"] != DBNull.Value
                            ? $"data:image/jpeg;base64,{Convert.ToBase64String((byte[])reader["CertificateThumbnail"])}"
                            : null
                    },
                    CommandType.StoredProcedure
                );
                return View("Index", list);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Sertifika Listeleme Hatası", ex.ToString());
                TempData["Type"] = "error";
                TempData["Message"] = "Sertifikalar listelenemedi.";
                return View("Index", new List<CertificateModel>());
            }
        }
        [HttpGet("Ekle")]
        public IActionResult Add() => View("Add");
        [HttpPost("Ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] CertificateUploadVm model)
        {
            ValidationResult validation = await _validator.ValidateAsync(model);
            if (!validation.IsValid)
                return Json(new { success = false, message = string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage)) });
            if (model.PdfDosyasi == null || model.PdfDosyasi.Length == 0)
                return Json(new { success = false, message = "PDF dosyası yüklenmedi." });
            try
            {
                using MemoryStream ms = new MemoryStream();
                await model.PdfDosyasi.CopyToAsync(ms);
                byte[] pdfBytes = ms.ToArray();
                byte[] thumbnailBytes = GenerateThumbnail(pdfBytes);
                List<SqlParameter> p = new()
        {
            new("@CertificateName", model.SertifikaAdi),
            new("@CertificateDescription", (object?)model.SertifikaAciklamasi ?? DBNull.Value),
            new("@CertificatePdf", pdfBytes),
            new("@CertificateThumbnail", thumbnailBytes),
            new("@UploaderIP", _http.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown"),
            new("@IsActive", model.IsActive)
        };

                await SQLCrud.InsertUpdateDeleteAsync(SP_INSERT, p);
                TempData["Type"] = "success";
                TempData["Message"] = "Sertifika başarıyla eklendi.";
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Sertifika Ekleme Hatası", ex.ToString());
                return Json(new { success = false, message = "Sertifika eklenemedi. " + ex.Message });
            }
        }
        [HttpGet("Duzenle/{id:int}")]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var result = await SQLCrud.ExecuteModelSingleAsync(
                    "dbo.CertificatesGetById",
                    new List<SqlParameter> { new("@Id", id) },
                    reader => new CertificateModel
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        CertificateName = reader["CertificateName"].ToString()!,
                        CertificateDescription = reader["CertificateDescription"].ToString(),
                        IsActive = Convert.ToBoolean(reader["IsActive"]),
                        ThumbnailBase64 = reader["CertificateThumbnail"] != DBNull.Value
                            ? $"data:image/jpeg;base64,{Convert.ToBase64String((byte[])reader["CertificateThumbnail"])}"
                            : null
                    },
                    CommandType.StoredProcedure
                );
                if (result == null)
                {
                    TempData["Type"] = "error";
                    TempData["Message"] = "Sertifika bulunamadı.";
                    return RedirectToAction("Index");
                }
                return View("Update", result);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Sertifika Getirme Hatası", ex.ToString());
                TempData["Type"] = "error";
                TempData["Message"] = "Sertifika bilgileri alınamadı.";
                return RedirectToAction("Index");
            }
        }
        [HttpPost("Duzenle/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, [FromForm] CertificateUploadVm model)
        {
            model.Id = id; 
            ValidationResult validation = await _validator.ValidateAsync(model);
            if (!validation.IsValid)
                return Json(new
                {
                    success = false,
                    message = string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage))
                });
            try
            {
                var existing = await SQLCrud.ExecuteModelSingleAsync(
                    "dbo.CertificatesGetById",
                    new List<SqlParameter> { new("@Id", id) },
                    reader => new
                    {
                        Pdf = reader["CertificatePdf"] != DBNull.Value ? (byte[])reader["CertificatePdf"] : Array.Empty<byte>(),
                        Thumb = reader["CertificateThumbnail"] != DBNull.Value ? (byte[])reader["CertificateThumbnail"] : Array.Empty<byte>()
                    },
                    CommandType.StoredProcedure
                );
                byte[] pdfBytes = existing?.Pdf ?? Array.Empty<byte>();
                byte[] thumbnailBytes = existing?.Thumb ?? Array.Empty<byte>();
                if (model.PdfDosyasi != null && model.PdfDosyasi.Length > 0)
                {
                    using MemoryStream ms = new MemoryStream();
                    await model.PdfDosyasi.CopyToAsync(ms);
                    pdfBytes = ms.ToArray();
                    try
                    {
                        thumbnailBytes = GenerateThumbnail(pdfBytes);
                    }
                    catch (Exception ex)
                    {
                        await Logging.LogAdd("Thumbnail Üretim Hatası", ex.ToString());
                        thumbnailBytes = existing?.Thumb ?? Array.Empty<byte>();
                    }
                }
                List<SqlParameter> p = new()
        {
            new("@Id", id),
            new("@CertificateName", model.SertifikaAdi),
            new("@CertificateDescription", (object?)model.SertifikaAciklamasi ?? DBNull.Value),
            new("@CertificatePdf", pdfBytes.Length > 0 ? pdfBytes : DBNull.Value),
            new("@CertificateThumbnail", thumbnailBytes.Length > 0 ? thumbnailBytes : DBNull.Value),
            new("@IsActive", model.IsActive)
        };

                await SQLCrud.InsertUpdateDeleteAsync(SP_UPDATE, p);
                return Json(new { success = true, message = "Sertifika başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Sertifika Güncelleme Hatası", ex.ToString());
                return Json(new { success = false, message = "Güncelleme sırasında hata oluştu. " + ex.Message });
            }
        }
        [HttpPost("Sil/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await SQLCrud.InsertUpdateDeleteAsync(SP_DELETE, new List<SqlParameter> { new("@Id", id) });
                return Json(new { success = true, message = "Sertifika başarıyla silindi." });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Sertifika Silme Hatası", ex.ToString());
                return Json(new { success = false, message = "Silme işlemi başarısız. " + ex.Message });
            }
        }
        [HttpPost("AktifPasifGuncelle/{id:int}")]
        public async Task<IActionResult> ToggleStatus(int id, [FromBody] ToggleStatusVm data)
        {
            if (data == null)
                return Json(new { success = false, message = "Geçersiz istek." });
            try
            {
                var p = new List<SqlParameter>
        {
            new("@Id", id),
            new("@IsActive", data.IsActive)
        };

                bool result = await SQLCrud.InsertUpdateDeleteAsync(SP_UPDATE_ISACTIVE, p);

                if (!result)
                    return Json(new { success = false, message = "Veritabanı güncellemesi başarısız." });
                string statusText = data.IsActive ? "aktif" : "pasif";
                return Json(new { success = true, message = $"Sertifika {statusText} hale getirildi." });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Aktif/Pasif Güncelleme Hatası", ex.ToString());
                return Json(new { success = false, message = "Durum güncellenemedi." });
            }
        }
        private byte[] GenerateThumbnail(byte[] pdfBytes)
        {
            try
            {
                string dllPath = Path.Combine(_env.ContentRootPath, "wwwroot", "ghostscript", "bin", "gsdll64.dll");
                string tempPdfPath = Path.Combine(Path.GetTempPath(), $"cert_{Guid.NewGuid()}.pdf");
                System.IO.File.WriteAllBytes(tempPdfPath, pdfBytes);
                using GhostscriptRasterizer rasterizer = new GhostscriptRasterizer();
                GhostscriptVersionInfo gsv = new GhostscriptVersionInfo(dllPath);
                rasterizer.Open(tempPdfPath, gsv, false);
                using Image img = rasterizer.GetPage(120, 1);
                using MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Jpeg);
                rasterizer.Close();
                System.IO.File.Delete(tempPdfPath);
                return ms.ToArray();
            }
            catch
            {
                return Array.Empty<byte>();
            }
        }
        [HttpGet("Indir/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                var result = await SQLCrud.ExecuteModelSingleAsync(
                    "dbo.CertificatesGetById",
                    new List<SqlParameter> { new("@Id", id) },
                    reader => new
                    {
                        Name = reader["CertificateName"].ToString(),
                        Pdf = reader["CertificatePdf"] != DBNull.Value ? (byte[])reader["CertificatePdf"] : null
                    },
                    CommandType.StoredProcedure
                );
                if (result == null || result.Pdf == null)
                    return NotFound("Dosya bulunamadı.");
                string fileName = $"{result.Name?.Replace(" ", "_") ?? "sertifika"}.pdf";
                return File(result.Pdf, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Sertifika PDF İndirme Hatası", ex.ToString());
                return StatusCode(500, "PDF indirilemedi.");
            }
        }
        [HttpGet("IndirmeLoglari")]
        public async Task<IActionResult> DownloadLogs()
        {
            try
            {
                var logs = await SQLCrud.ExecuteModelListAsync<CertificateDownloadLogModel>(
                    "dbo.CertificateDownloadLogGet",
                    null,
                    reader => new CertificateDownloadLogModel
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        CertificateId = Convert.ToInt32(reader["CertificateId"]),
                        CertificateName = reader["CertificateName"]?.ToString() ?? "Bilinmiyor",
                        DownloadUtc = reader["DownloadUtc"] != DBNull.Value
                                        ? Convert.ToDateTime(reader["DownloadUtc"])
                                        : DateTime.MinValue,
                        ClientIp = reader["ClientIp"]?.ToString() ?? "-",
                        City = reader["City"]?.ToString(),
                        UserAgent = reader["UserAgent"]?.ToString()
                    },
                    CommandType.StoredProcedure
                );
                return View("DownloadLogs", logs);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Sertifika İndirme Log Listeleme Hatası", ex.ToString());
                TempData["Type"] = "error";
                TempData["Message"] = "Log kayıtları alınamadı.";
                return View("DownloadLogs", new List<CertificateDownloadLogModel>());
            }
        }
        [HttpPost("IndirmeLogSil/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDownloadLog(int id)
        {
            try
            {
                bool result = await SQLCrud.InsertUpdateDeleteAsync(
                    "dbo.CertificateDownloadLogDelete",
                    new List<SqlParameter> { new("@Id", id) }
                );
                if (result)
                    return Json(new { success = true, message = "Log kaydı başarıyla silindi." });
                else
                    return Json(new { success = false, message = "Log kaydı silinemedi." });
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("İndirme Log Silme Hatası", ex.ToString());
                return Json(new { success = false, message = "Silme sırasında hata oluştu. " + ex.Message });
            }
        }
        public class ToggleStatusVm
        {
            public bool IsActive { get; set; }
        }
    }
}