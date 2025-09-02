using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace MyWebSite.Controllers
{
    [Route("AdminDosya")]
    [Authorize(Roles = "Admin")]
    public class AdminFileStoreController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _http;
        private const string SP_ADD = "dbo.FileStore_Add";
        private const string SP_LIST = "dbo.FileStore_List";
        private const string SP_GETBYID = "dbo.FileStore_GetById";
        private const string SP_DWREG = "dbo.FileStore_RegisterDownload";
        private const string SP_DELETE = "dbo.FileStore_Delete";
        private const string SP_GET_BY_KVF = "dbo.FileStore_GetByKeyVersionFile"; 
        private const string SP_SET_ACTIVE = "dbo.FileStore_SetActive";          
        private const string SP_DL_LOG = "dbo.FileDownloadLog_List";
        private const string SP_DL_DELETE = "dbo.FileDownloadLog_Delete";
        private static readonly Regex VersionStrict = new(@"^\d+\.\d+\.\d+$", RegexOptions.CultureInvariant);
        private static readonly Regex AllowedSegment = new(@"^[A-Za-z0-9._-]{1,64}$", RegexOptions.CultureInvariant);
        public AdminFileStoreController(IWebHostEnvironment env, IHttpContextAccessor http)
        {
            _env = env;
            _http = http;
        }
        [HttpGet("IndirmeLog")]
        public async Task<IActionResult> IndirmeLog(
            string? packageKey = null,
            string? version = null,
            int? fileId = null,
            DateTime? fromUtc = null,
            DateTime? toUtc = null,
            string? q = null 
        )
        {
            List<SqlParameter> p = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(packageKey)) p.Add(new("@PackageKey", packageKey));
            if (!string.IsNullOrWhiteSpace(version)) p.Add(new("@Version", version));
            if (fileId.HasValue) p.Add(new("@FileId", fileId.Value));
            if (fromUtc.HasValue) p.Add(new("@FromUtc", fromUtc.Value));
            if (toUtc.HasValue) p.Add(new("@ToUtc", toUtc.Value));
            if (!string.IsNullOrWhiteSpace(q)) p.Add(new("@Search", q));
            var list = await SQLCrud.ExecuteModelListAsync<DownloadLogItem>(
                SP_DL_LOG,
                p,
                r => new DownloadLogItem
                {
                    Id = r.GetInt32(r.GetOrdinal("Id")),
                    FileId = r.GetInt32(r.GetOrdinal("FileId")),
                    PackageKey = r["PackageKey"].ToString()!,
                    Version = r["Version"].ToString()!,
                    FileNameStored = r["FileNameStored"].ToString()!,
                    DownloadUtc = (DateTime)r["DownloadUtc"],
                    ClientIp = r["ClientIp"].ToString()!,
                    UserAgent = r["UserAgent"] as string,
                    DownloadIndex = r.GetInt32(r.GetOrdinal("DownloadIndex")),
                    City = SafeGetString(r, "City") 
                }
            );
            return View("IndirmeLog", list);
        }
        [HttpPost("DeleteDownloadLog/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDownloadLog(int id)
        {
            string ip = _http?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            bool ok;
            try
            {
                ok = await SQLCrud.InsertUpdateDeleteAsync(SP_DL_DELETE, new List<SqlParameter>
        {
            new("@Id", id),
            new("@ClientIp", ip)
        });
            }
            catch (Exception ex)
            {
                ok = false;
                await Logging.LogAdd("[DL LOG DELETE ERROR]", ex.ToString());
            }
            TempData["Type"] = ok ? "success" : "error";
            TempData["Message"] = ok ? "Log silindi." : "Log silinemedi.";
            return RedirectToAction(nameof(IndirmeLog));
        }
        private static string? SafeGetString(System.Data.IDataRecord r, string col)
        {
            for (int i = 0; i < r.FieldCount; i++)
                if (string.Equals(r.GetName(i), col, StringComparison.OrdinalIgnoreCase))
                    return r.IsDBNull(i) ? null : r.GetString(i);
            return null;
        }
        private static bool IsAjaxRequest(HttpRequest req) =>
            string.Equals(req.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        [HttpGet("Liste")]
        public async Task<IActionResult> Index(string? packageKey = null)
        {
            List<SqlParameter> p = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(packageKey))
                p.Add(new("@PackageKey", packageKey));
            var list = await SQLCrud.ExecuteModelListAsync<FileListItem>(
                SP_LIST, p,
                r => new FileListItem
                {
                    Id = r.GetInt32(r.GetOrdinal("Id")),
                    PackageKey = r["PackageKey"].ToString(),
                    Version = r["Version"].ToString(),
                    ArchiveType = r["ArchiveType"].ToString(),
                    FileNameOriginal = r["FileNameOriginal"].ToString(),
                    FileNameStored = r["FileNameStored"].ToString(),
                    RelativePath = r["RelativePath"].ToString(),
                    FileSizeBytes = (long)r["FileSizeBytes"],
                    Sha256 = r["Sha256"].ToString(),
                    UploadUtc = (DateTime)r["UploadUtc"],
                    LastModifiedUtc = (DateTime)r["LastModifiedUtc"],
                    IsActive = (bool)r["IsActive"],
                    DownloadCount = (int)r["DownloadCount"]
                });
            return View(list);
        }
        [HttpGet("Yukle")]
        public IActionResult Upload() => View(new FileUploadVm());
        [HttpPost("Yukle")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1L * 1024 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1L * 1024 * 1024 * 1024)]
        public async Task<IActionResult> Upload(FileUploadVm vm)
        {
            if (!ModelState.IsValid)
                return FailResultFromModelState(vm);
            if (vm.Archive is null || vm.Archive.Length == 0)
                return FailResult("Dosya seçilmedi.", vm, nameof(vm.Archive));
            string? ext = Path.GetExtension(vm.Archive.FileName).ToLowerInvariant();
            if (ext is not (".zip" or ".rar"))
                return FailResult("Sadece .zip veya .rar kabul edilir.", vm, nameof(vm.Archive));
            if (string.IsNullOrWhiteSpace(vm.DownloadPasswordPlain) || vm.DownloadPasswordPlain.Length < 6)
                return FailResult("Şifre en az 6 karakter olmalı.", vm, nameof(vm.DownloadPasswordPlain));
            string safePackage = SanitizeSegmentStrict(vm.PackageKey);
            string version = string.IsNullOrWhiteSpace(vm.Version) ? "1.0.0" : vm.Version.Trim();
            if (!VersionStrict.IsMatch(version))
                return FailResult("Versiyon biçimi 1.2.3 olmalı.", vm, nameof(vm.Version));
            string contentRoot = _env.ContentRootPath ?? Directory.GetCurrentDirectory();
            string tempDir = Path.Combine(contentRoot, "App_Data", "uploads_tmp");
            Directory.CreateDirectory(tempDir);
            string tmpName = Path.GetRandomFileName() + ext;
            string tmpPath = Path.Combine(tempDir, tmpName);
            try
            {
                await using var outFs = System.IO.File.Create(tmpPath);
                await vm.Archive.CopyToAsync(outFs, HttpContext.RequestAborted);
            }
            catch (Exception ex)
            {
                TryDelete(tmpPath);
                await Logging.LogAdd("[UPLOAD IO ERROR]", ex.ToString());
                return FailResult("Dosya yazılırken hata oluştu.", vm);
            }
            try
            {
                if (!await ValidateArchiveSignatureOnDiskAsync(tmpPath, ext, HttpContext.RequestAborted))
                {
                    TryDelete(tmpPath);
                    return FailResult("Arşiv imzası geçersiz (dosya bozuk ya da uzantı sahte).", vm, nameof(vm.Archive));
                }
            }
            catch (Exception ex)
            {
                TryDelete(tmpPath);
                await Logging.LogAdd("[SIGNATURE CHECK ERROR]", ex.ToString());
                return FailResult("Arşiv doğrulanamadı.", vm);
            }
            string sha256;
            try
            {
                sha256 = await ComputeSha256Async(tmpPath);
            }
            catch (Exception ex)
            {
                TryDelete(tmpPath);
                await Logging.LogAdd("[SHA256 ERROR]", ex.ToString());
                return FailResult("Hash hesaplanamadı.", vm);
            }
            string archiveType = ext.TrimStart('.');
            string webRoot = _env.WebRootPath ?? Path.Combine(contentRoot, "wwwroot");
            string baseDir = Path.Combine(webRoot, "filestore", safePackage, version);
            Directory.CreateDirectory(baseDir);
            string safeOriginal = MakeSafeFileName(vm.Archive.FileName);
            string storedName = $"{sha256[..12]}_{safeOriginal}";
            string fullPath = Path.Combine(baseDir, storedName);
            try
            {
                if (await ExistsByPVF(safePackage, version, storedName))
                {
                    TryDelete(tmpPath);
                    return FailResult("Aynı paket ve sürümde aynı dosya adı zaten mevcut.", vm);
                }
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("[DUPLICATE CHECK WARN]", ex.Message);
            }
            if (System.IO.File.Exists(fullPath))
            {
                storedName = $"{sha256[..12]}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{ext}";
                fullPath = Path.Combine(baseDir, storedName);
            }
            try
            {
                System.IO.File.Move(tmpPath, fullPath);
            }
            catch (Exception ex)
            {
                TryDelete(tmpPath);
                await Logging.LogAdd("[MOVE ERROR]", ex.ToString());
                return FailResult("Dosya taşınamadı.", vm);
            }
            FileInfo fi = new FileInfo(fullPath);
            long sizeBytes = fi.Length;
            DateTime lastUtc = fi.LastWriteTimeUtc;
            string passHash = HashingControl.HashPassword(vm.DownloadPasswordPlain);
            string relative = Path.Combine("filestore", safePackage, version, storedName).Replace('\\', '/');
            string ip = _http?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new("@PackageKey",           safePackage),
                new("@Version",              version),
                new("@ArchiveType",          archiveType),
                new("@FileNameOriginal",     vm.Archive.FileName),
                new("@FileNameStored",       storedName),
                new("@RelativePath",         relative),
                new("@FileSizeBytes",        sizeBytes),
                new("@Sha256",               sha256),
                new("@LastModifiedUtc",      lastUtc),
                new("@DownloadPasswordHash", passHash),
                new("@UploadedByIp",         ip),
                new("@NewId", SqlDbType.Int){ Direction = ParameterDirection.Output }
            };
            bool ok;
            try
            {
                ok = await SQLCrud.InsertUpdateDeleteAsync(SP_ADD, parameters);
            }
            catch (Exception ex)
            {
                ok = false;
                await Logging.LogAdd("[DB INSERT ERROR]", ex.ToString());
            }
            if (!ok)
            {
                TryDelete(fullPath);
                return FailResult("DB ekleme başarısız.", vm);
            }
            if (IsAjaxRequest(Request))
                return Json(new { ok = true, redirect = Url.Action("Liste", "AdminDosya") });
            TempData["Type"] = "success";
            TempData["Message"] = $"Yüklendi • Paket:{safePackage} Versiyon:{version}";
            return RedirectToAction(nameof(Index), new { packageKey = safePackage });
        }
        [HttpGet("Indir/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            FileDetails file = await GetByIdAsync(id);
            if (file is null || !file.IsActive) return NotFound();
            return View("DownloadPrompt", new DownloadPromptVm
            {
                Id = file.Id,
                PackageKey = file.PackageKey,
                Version = file.Version,
                FileNameStored = file.FileNameStored,
                ArchiveType = file.ArchiveType
            });
        }
        [HttpPost("Indir/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Download(int id, string password)
        {
            FileDetails file = await GetByIdAsync(id);
            if (file is null || !file.IsActive) return NotFound();
            if (string.IsNullOrWhiteSpace(password) ||
                !HashingControl.VerifyPassword(password, file.DownloadPasswordHash))
            {
                await Logging.LogAdd("[DOWNLOAD FAIL]", $"Wrong password for Id:{id}");
                TempData["Type"] = "error";
                TempData["Message"] = "Parola hatalı.";
                return RedirectToAction(nameof(Get), new { id });
            }
            string fullPath = Path.Combine(
                _env.WebRootPath ?? Path.Combine(_env.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot"),
                file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fullPath))
            {
                await Logging.LogAdd("[DOWNLOAD MISSING]", $"File not found Id:{id} Path:{fullPath}");
                return NotFound();
            }
            string? city = GetCityFromHeaders(Request);
            if (string.IsNullOrWhiteSpace(city))
            {
                string? ip = GetClientIp(HttpContext);
            }
            await SQLCrud.InsertUpdateDeleteAsync("dbo.FileStore_RegisterDownload", new()
{
    new("@Id", id),
    new("@ClientIp", GetClientIp(HttpContext) ?? "unknown"),
    new("@UserAgent", Request.Headers.UserAgent.ToString()),
    new("@City", (object?)city ?? DBNull.Value)
});
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["X-Frame-Options"] = "DENY";
            Response.Headers["Referrer-Policy"] = "no-referrer";
            string contentType = file.ArchiveType?.ToLowerInvariant() switch
            {
                "zip" => "application/zip",
                "rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
            return PhysicalFile(fullPath, contentType, file.FileNameStored, enableRangeProcessing: true);
        }
        private static string? GetHeader(HttpRequest req, string name)
    => req.Headers.TryGetValue(name, out var v) ? v.ToString() : null;
        private static string? GetCityFromHeaders(HttpRequest req)
        {
            return GetHeader(req, "CF-IPCity")
                ?? GetHeader(req, "CloudFront-Viewer-City")  
                ?? GetHeader(req, "X-AppEngine-City");     
        }
        private static string? GetClientIp(HttpContext ctx)
        {
            string? ip = GetHeader(ctx.Request, "CF-Connecting-IP");
            if (string.IsNullOrWhiteSpace(ip))
                ip = GetHeader(ctx.Request, "X-Forwarded-For")?.Split(',').FirstOrDefault()?.Trim();
            return ip ?? ctx.Connection.RemoteIpAddress?.ToString();
        }
        [HttpPost("Sil/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            FileDetails file = await GetByIdAsync(id);
            if (file is null) return NotFound();
            string ip = _http?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            bool ok = await SQLCrud.InsertUpdateDeleteAsync(SP_DELETE, new List<SqlParameter>
            {
                new("@Id", id),
                new("@ClientIp", ip)
            });
            try
            {
                string fullPath = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(_env.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot"),
                    file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("[DELETE IO WARN]", ex.Message);
            }
            TempData["Type"] = ok ? "success" : "error";
            TempData["Message"] = ok ? "Silindi." : "Silme başarısız.";
            return RedirectToAction(nameof(Index));
        }
        [HttpPost("Durum")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetActive(int id)
        {
            FileDetails file = await GetByIdAsync(id);
            if (file is null)
            {
                TempData["Type"] = "error";
                TempData["Message"] = "Kayıt bulunamadı.";
                return RedirectToAction(nameof(Index));
            }
            bool target = !file.IsActive; 
            string ip = _http?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            bool ok;
            try
            {
                ok = await SQLCrud.InsertUpdateDeleteAsync(SP_SET_ACTIVE, new List<SqlParameter>
        {
            new("@Id", id),
            new("@IsActive", target),
            new("@ClientIp", ip)
        });
            }
            catch (Exception ex)
            {
                ok = false;
                await Logging.LogAdd("[SET ACTIVE ERROR]", ex.ToString());
            }
            TempData["Type"] = ok ? "success" : "error";
            TempData["Message"] = ok
                ? (target ? "Aktif edildi." : "Pasif edildi.")
                : "Durum güncellenemedi.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("Ham/{id:int}")]
        public async Task<IActionResult> Direct(int id)
        {
            FileDetails file = await GetByIdAsync(id);
            if (file is null) return NotFound();
            string? root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath ?? Directory.GetCurrentDirectory(), "wwwroot");
            string? fullPath = Path.Combine(root, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fullPath)) return NotFound();
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["X-Frame-Options"] = "DENY";
            Response.Headers["Referrer-Policy"] = "no-referrer";
            string? contentType = (file.ArchiveType ?? string.Empty).ToLowerInvariant() switch
            {
                "zip" => "application/zip",
                "rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
            return PhysicalFile(fullPath, contentType, file.FileNameStored, enableRangeProcessing: true);
        }
        private async Task<FileDetails?> GetByIdAsync(int id)
        {
            var list = await SQLCrud.ExecuteModelListAsync<FileDetails>(
                SP_GETBYID,
                new List<SqlParameter> { new("@Id", id) },
                r => new FileDetails
                {
                    Id = r.GetInt32(r.GetOrdinal("Id")),
                    PackageKey = r["PackageKey"].ToString(),
                    Version = r["Version"].ToString(),
                    ArchiveType = r["ArchiveType"].ToString(),
                    FileNameOriginal = r["FileNameOriginal"].ToString(),
                    FileNameStored = r["FileNameStored"].ToString(),
                    RelativePath = r["RelativePath"].ToString(),
                    FileSizeBytes = (long)r["FileSizeBytes"],
                    Sha256 = r["Sha256"].ToString(),
                    UploadUtc = (DateTime)r["UploadUtc"],
                    LastModifiedUtc = (DateTime)r["LastModifiedUtc"],
                    DownloadPasswordHash = r["DownloadPasswordHash"].ToString(),
                    IsActive = (bool)r["IsActive"],
                    DownloadCount = (int)r["DownloadCount"]
                });
            return list.FirstOrDefault();
        }
        private async Task<bool> ExistsByPVF(string packageKey, string version, string fileNameStored)
        {
            var list = await SQLCrud.ExecuteModelListAsync<FileDetails>(
                SP_GET_BY_KVF,
                new List<SqlParameter>
                {
                    new("@PackageKey", packageKey),
                    new("@Version", version),
                    new("@FileNameStored", fileNameStored),
                },
                r => new FileDetails { Id = r.GetInt32(r.GetOrdinal("Id")) }
            );
            return list.Any();
        }
        private static string SanitizeSegmentStrict(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Package";
            string? s = input.Trim();
            return AllowedSegment.IsMatch(s) ? s : "Package";
        }
        private static string MakeSafeFileName(string fileName)
        {
            string name = Path.GetFileName(fileName);
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }
        private static void TryDelete(string path)
        {
            try { if (System.IO.File.Exists(path)) System.IO.File.Delete(path); }
            catch { }
        }
        private static async Task<string> ComputeSha256Async(string filePath)
        {
            await using var stream = System.IO.File.OpenRead(filePath);
            using var sha = SHA256.Create();
            byte[] hash = await sha.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        private static async Task<bool> ValidateArchiveSignatureOnDiskAsync(string path, string ext, CancellationToken ct)
        {
            byte[] header = new byte[8];
            await using var s = System.IO.File.OpenRead(path);
            int read = await s.ReadAsync(header.AsMemory(0, header.Length), ct);
            return ext switch
            {
                ".zip" => LooksLikeZip(header.AsSpan(0, read)),
                ".rar" => LooksLikeRar(header.AsSpan(0, read)),
                _ => false
            };
        }
        private static bool LooksLikeZip(ReadOnlySpan<byte> h)
            => h.Length >= 4 &&
               h[0] == 0x50 && h[1] == 0x4B &&                  
               (h[2] == 0x03 || h[2] == 0x05 || h[2] == 0x07) &&
               (h[3] == 0x04 || h[3] == 0x06 || h[3] == 0x08);
        private static bool LooksLikeRar(ReadOnlySpan<byte> h)
            => h.Length >= 7 &&
               h[0] == 0x52 && h[1] == 0x61 && h[2] == 0x72 &&     
               h[3] == 0x21 && h[4] == 0x1A && h[5] == 0x07 &&
               (h[6] == 0x00 || h[6] == 0x01);                     
        private IActionResult FailResultFromModelState(FileUploadVm vm)
        {
            if (IsAjaxRequest(Request))
            {
                var errors = ModelState
                    .Where(kv => kv.Value!.Errors.Count > 0)
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                Response.StatusCode = 422;
                return Json(new { ok = false, errors });
            }
            TempData["Type"] = "error";
            TempData["Message"] = "Form hatalı. Alanları kontrol edin.";
            return View("Upload", vm);
        }
        private IActionResult FailResult(string message, FileUploadVm vm, string? field = null)
        {
            if (IsAjaxRequest(Request))
            {
                Response.StatusCode = 422; 
                return Json(new { ok = false, message, field });
            }
            TempData["Type"] = "error";
            TempData["Message"] = message;
            if (!string.IsNullOrEmpty(field)) ModelState.AddModelError(field, message);
            else ModelState.AddModelError(string.Empty, message);
            return View("Upload", vm);
        }
    }
}