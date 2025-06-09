using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class JobComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                string sql = "JobsGet";
                List<SqlParameter> parameters = new List<SqlParameter>();
                List<Jobs> jobs = await SQLCrud.ExecuteModelListAsync<Jobs>(
                    sql,
                    parameters,
                    reader => new Jobs
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        JobName = reader["JobName"].ToString(),
                        JobTitle = reader["JobTitle"].ToString(),
                        JobYears = reader["JobYears"].ToString(),
                        JobAbout = reader["JobAbout"].ToString()
                    },
                    CommandType.StoredProcedure
                );
                return View(jobs);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfa Eğitim Listeleme İşlemi Hatası", ex.Message);
                return View(new List<Jobs>());
            }
        }
    }
}