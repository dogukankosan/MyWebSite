using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyWebSite.Controllers
{
    [Route("d")]
    public class DownloadController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _http;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string SP_ADMIN_LOGS_ADD = "dbo.AdminLogsAdd";
        private const string SP_PUBLIC_LIST = "dbo.FileStore_PublicList";
        private const string SP_GET_BY_KV = "dbo.FileStore_GetByKeyVersion";
        private const string SP_GET_BY_KVF = "dbo.FileStore_GetByKeyVersionFile";
        private const string SP_DWREG = "dbo.FileStore_RegisterDownload";
        private static readonly Regex VersionStrict =
            new(@"^\d+\.\d+\.\d+$", RegexOptions.CultureInvariant);
        private static readonly Regex FileNameAllowed =
            new(@"^[^\\/:*?""<>|]+\.(zip|rar)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex PackageKeyAllowed =
            new(@"^[A-Za-z0-9._-]{1,64}$", RegexOptions.CultureInvariant);
        public DownloadController(IWebHostEnvironment env, IHttpContextAccessor http, IHttpClientFactory httpClientFactory)
        {
            _env = env;
            _http = http;
            _httpClientFactory = httpClientFactory;
        }
        private async Task SafeAdminLog(string logType, string message)
        {
            try
            {
                string? ip = GetClientIp(HttpContext) ?? "unknown";
                if (ip.Length > 25) ip = ip.Substring(0, 25);

                await SQLCrud.InsertUpdateDeleteAsync(SP_ADMIN_LOGS_ADD, new()
        {
            new("@LogType",     logType),
            new("@ErrorMessage",message),
            new("@IPAdress",    ip)
        });
            }
            catch {  }
        }
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(string? packageKey = null, string? version = null, string? q = null)
        {
            SetCommonSecurityHeaders();
            List<SqlParameter> p = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(packageKey))
            {
                packageKey = packageKey.Trim();
                if (!PackageKeyAllowed.IsMatch(packageKey))
                {
                    TempData["Type"] = "error";
                    TempData["Message"] = "Geçersiz paket anahtarı.";
                    return RedirectToAction(nameof(Index), new { version, q = q ?? "" });
                }
                p.Add(new("@PackageKey", packageKey));
            }
            if (!string.IsNullOrWhiteSpace(version))
            {
                version = version.Trim();
                if (!VersionStrict.IsMatch(version))
                {
                    TempData["Type"] = "error";
                    TempData["Message"] = "Versiyon biçimi 1.2.3 olmalı.";
                    return RedirectToAction(nameof(Index), new { packageKey, q });
                }
                p.Add(new("@Version", version));
            }
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                if (!FileNameAllowed.IsMatch(q))
                {
                    TempData["Type"] = "error";
                    TempData["Message"] = "Dosya adı .zip veya .rar ile bitmelidir (ör. paket-1.0.1.zip).";
                    return RedirectToAction(nameof(Index), new { packageKey, version });
                }
                p.Add(new("@Search", q));
            }
            var list = await SQLCrud.ExecuteModelListAsync<FileListItem>(
                SP_PUBLIC_LIST,
                p,
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
            return View("Index", list);
        }
        [HttpGet("{packageKey}/{version:regex(^\\d+\\.\\d+\\.\\d+$)}")]
        public async Task<IActionResult> PromptKv(string packageKey, string version, [FromQuery] string? fileName)
        {
            SetCommonSecurityHeaders();
            if (!PackageKeyAllowed.IsMatch(packageKey))
                return BadRequest("Geçersiz paket anahtarı.");
            if (!string.IsNullOrWhiteSpace(fileName) && !FileNameAllowed.IsMatch(fileName))
                return BadRequest("Geçersiz dosya adı.");
            return await PromptInternal(packageKey, version, fileName);
        }
        private async Task<IActionResult> PromptInternal(string packageKey, string version, string? fileName)
        {
            FileDetails? file = await FindFile(packageKey, version, fileName);
            if (file is null || !file.IsActive) return NotFound();
            DownloadPromptVm vm = new DownloadPromptVm
            {
                Id = file.Id,
                PackageKey = file.PackageKey,
                Version = file.Version,
                FileNameStored = file.FileNameStored,
                ArchiveType = file.ArchiveType
            };
            return View("Prompt", vm); 
        }
        [HttpPost("{packageKey}/{version:regex(^\\d+\\.\\d+\\.\\d+$)}")]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> DownloadKv(string packageKey, string version, string? fileName, string password)
            => DownloadInternal(packageKey, version, fileName, password);
        private async Task<IActionResult> DownloadInternal(string packageKey, string version, string? fileName, string password)
        {
            string dlToken = Request.Form["dlToken"].ToString();
            CookieOptions cookieOpts = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddMinutes(3),
                HttpOnly = false,            
                SameSite = SameSiteMode.Strict,   
                Secure = Request.IsHttps,      
                Path = "/"
            };
            if (!PackageKeyAllowed.IsMatch(packageKey))
            {
                if (!string.IsNullOrEmpty(dlToken)) Response.Cookies.Append($"dl_{dlToken}", "err", cookieOpts);
                return RedirectToAction(nameof(PromptKv), new { packageKey, version });
            }
            if (!string.IsNullOrWhiteSpace(fileName) && !FileNameAllowed.IsMatch(fileName))
            {
                if (!string.IsNullOrEmpty(dlToken)) Response.Cookies.Append($"dl_{dlToken}", "err", cookieOpts);
                return RedirectToAction(nameof(PromptKv), new { packageKey, version });
            }
            FileDetails? file = await FindFile(packageKey, version, fileName);
            if (file is null || !file.IsActive) return NotFound();
            if (string.IsNullOrWhiteSpace(password) ||
                !HashingControl.VerifyPassword(password, file.DownloadPasswordHash))
            {   
                await SafeAdminLog(
                    "Hatalı İndirme Şifre Girişi",
                    $"Wrong password. pkg={packageKey}, ver={version}, file={file.FileNameStored}, ua={Request.Headers.UserAgent}"
                );
                await Task.Delay(Random.Shared.Next(250, 600));
                if (!string.IsNullOrEmpty(dlToken)) Response.Cookies.Append($"dl_{dlToken}", "err", cookieOpts);
                return RedirectToAction(nameof(PromptKv), new { packageKey, version, fileName });
            }
            string? root = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            string? fullPath = Path.Combine(root, file.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            string? rootNorm = Path.GetFullPath(root);
            string? fullNorm = Path.GetFullPath(fullPath);
            if (!fullNorm.StartsWith(rootNorm, StringComparison.OrdinalIgnoreCase))
                return NotFound();
            if (!System.IO.File.Exists(fullNorm)) return NotFound();
            string? city = GetCityFromHeaders(Request);
            if (string.IsNullOrWhiteSpace(city))
            {
                string? ip = GetClientIp(HttpContext);
                if (!string.IsNullOrWhiteSpace(ip) && IsPublicIp(ip))
                    city = await ResolveCityFromIpAsync(ip);
            }
            await SQLCrud.InsertUpdateDeleteAsync("dbo.FileStore_RegisterDownload", new()
{
    new("@Id", file.Id),
    new("@ClientIp", GetClientIp(HttpContext) ?? "unknown"),
    new("@UserAgent", Request.Headers.UserAgent.ToString()),
    new("@City", (object?)city ?? DBNull.Value)
});
            if (!string.IsNullOrEmpty(dlToken))
                Response.Cookies.Append($"dl_{dlToken}", "ok", cookieOpts);
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Referrer-Policy"] = "no-referrer";
            Response.Headers["Content-Security-Policy"] = "frame-ancestors 'self'";
            Response.Headers["Cache-Control"] = "no-store, private";
            if (!string.IsNullOrWhiteSpace(file.Sha256))
                Response.Headers["ETag"] = $"W/\"{file.Sha256}\"";
            string ascii = ToAsciiFilename(file.FileNameStored);
            Response.Headers["Content-Disposition"] =
                $"attachment; filename=\"{ascii}\"; filename*=UTF-8''{Uri.EscapeDataString(file.FileNameStored)}";
            string contentType = GetContentType(file.ArchiveType);
            return PhysicalFile(fullNorm, contentType, file.FileNameStored, enableRangeProcessing: true);
        }
        private async Task<string?> ResolveCityFromIpAsync(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip)) return null;
            if (!IsPublicIp(ip)) return null;
            string url = $"http://ip-api.com/json/{ip}?fields=status,city";
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                using HttpResponseMessage resp = await client.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return null;
                using Stream stream = await resp.Content.ReadAsStreamAsync();
                using JsonDocument doc = await JsonDocument.ParseAsync(stream);
                JsonElement root = doc.RootElement;
                if (root.TryGetProperty("status", out var st) &&
                    st.GetString()?.Equals("success", StringComparison.OrdinalIgnoreCase) == true &&
                    root.TryGetProperty("city", out var cityEl))
                {
                    string city = cityEl.GetString();
                    return string.IsNullOrWhiteSpace(city) ? null : city;
                }
            }
            catch
            {
                
            }
            return null;
        }
        private static bool IsPublicIp(string ipString)
        {
            if (!IPAddress.TryParse(ipString, out var ip)) return false;
            if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();
            if (ip.AddressFamily == AddressFamily.InterNetwork) 
            {
                byte[] b = ip.GetAddressBytes();
                if (b[0] == 10) return false;
                if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return false;
                if (b[0] == 192 && b[1] == 168) return false;
                if (b[0] == 127) return false;
                return true;
            }
            if (ip.AddressFamily == AddressFamily.InterNetworkV6) 
            {
                if (ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || IPAddress.IsLoopback(ip)) return false;
                byte[] b = ip.GetAddressBytes();
                if ((b[0] & 0xFE) == 0xFC) return false;
                return true;
            }
            return false;
        }
        private async Task<FileDetails?> FindFile(string packageKey, string version, string? fileName)
        {
            List<SqlParameter> p = new List<SqlParameter>
            {
                new("@PackageKey", packageKey),
                new("@Version", version)
            };
            string sp = SP_GET_BY_KV;
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                sp = SP_GET_BY_KVF;
                p.Add(new("@FileNameStored", fileName));
            }
            var list = await SQLCrud.ExecuteModelListAsync<FileDetails>(
                sp, p,
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
        private static string GetContentType(string archiveType)
            => archiveType?.ToLowerInvariant() switch
            {
                "zip" => "application/zip",
                "rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };
        private void SetCommonSecurityHeaders()
        {
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            Response.Headers["Referrer-Policy"] = "no-referrer";
            Response.Headers["Content-Security-Policy"] = "frame-ancestors 'self'";
        }
        private static string ToAsciiFilename(string name)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(name);
            for (int i = 0; i < bytes.Length; i++)
                if (bytes[i] < 32 || bytes[i] > 126) bytes[i] = (byte)'_';
            return Encoding.ASCII.GetString(bytes);
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
    }
}