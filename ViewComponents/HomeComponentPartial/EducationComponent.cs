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
                List<SqlParameter> parameters = new List<SqlParameter>();
                List<Education> educations = await SQLCrud.ExecuteModelListAsync<Education>(
                    sql,
                    parameters,
                    reader => new Education
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        SchoolName = reader["SchoolName"].ToString(),
                        SectionName = reader["SectionName"].ToString(),
                        Years = reader["Years"].ToString()
                    },
                    CommandType.StoredProcedure
                );
                return View(educations);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfada Eğitim Listeleme İşlemi Hatası", ex.Message);
                return View(new List<Education>());
            }
        }
    }
}