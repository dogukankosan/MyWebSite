using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class EducationComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                string sql = "EducationGet";
                var educations = await SQLCrud.ExecuteModelListAsync<Education>(
                    sql,
                    null,
                    reader => new Education
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        SchoolName = reader["SchoolName"].ToString(),
                        SectionName = reader["SectionName"].ToString(),
                        Years = reader["Years"].ToString(),
                        Status = reader["Status"] != DBNull.Value && Convert.ToBoolean(reader["Status"])
                    },
                    CommandType.StoredProcedure
                );
                var activeList = educations
                    .Where(e => e.Status)
                    .OrderByDescending(e => e.ID) 
                    .ToList();
                return View(activeList);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfada Eğitim Listeleme İşlemi Hatası", ex.Message);
                return View(new List<Education>());
            }
        }
    }
}