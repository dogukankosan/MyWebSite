using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class CertificatesComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                string sql = "CertificatesGet";
                List<CertificateModel> certificates = await SQLCrud.ExecuteModelListAsync<CertificateModel>(
                    sql,
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
                            : "/userThema/img/noimage.png"
                    },
                    CommandType.StoredProcedure
                );
                var activeCerts = certificates
                    .Where(x => x.IsActive)
                    .OrderByDescending(x => x.UploadDate) 
                    .ToList();
                return View(activeCerts);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfa Sertifika Listeleme Hatası", ex.ToString());
                return View(new List<CertificateModel>());
            }
        }
    }
}