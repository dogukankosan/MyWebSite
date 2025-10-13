using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.Controllers
{
    [AllowAnonymous]
    [Route("Certificate")]
    public class CertificateDownloadController : Controller
    {
        [HttpGet("Download/{id:int}")]
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                string? ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                string? userAgent = Request.Headers["User-Agent"].ToString();
                string city = "";
                try
                {
                    using HttpClient client = new();
                    string response = await client.GetStringAsync($"http://ip-api.com/json/{ip}");
                    JObject json = JObject.Parse(response);
                    city = $"{json["city"]}, {json["country"]}";
                }
                catch
                {
                    city = "Bilinmiyor";
                }
                string logProc = "CertificateDownloadLogInsert";
                List<SqlParameter> logParams = new()
        {
            new("@CertificateId", id),
            new("@ClientIp", ip ?? (object)DBNull.Value),
            new("@UserAgent", userAgent ?? (object)DBNull.Value),
            new("@City", city ?? (object)DBNull.Value),
            new("@DownloadIndex", 1)
        };
                await SQLCrud.InsertUpdateDeleteAsync(logProc, logParams);
                var result = await SQLCrud.ExecuteModelSingleAsync(
                    "dbo.CertificatesGetById",
                    new List<SqlParameter> { new("@Id", id) },
                    reader => new
                    {
                        Name = reader["CertificateName"].ToString(),
                        Pdf = reader["CertificatePdf"] != DBNull.Value ? (byte[])reader["CertificatePdf"] : null,
                        Status = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"])
                    },
                    CommandType.StoredProcedure
                );
                if (result == null || result.Pdf == null || !result.Status)
                    return NotFound("Bu sertifika aktif değil veya mevcut değil.");
                string fileName = $"{result.Name?.Replace(" ", "_") ?? "Sertifika"}.pdf";
                return File(result.Pdf, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Sertifika İndirme Hatası", ex.ToString());
                return StatusCode(500, "Sertifika indirilemedi.");
            }
        }
    }
}