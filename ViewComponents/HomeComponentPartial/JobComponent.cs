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
                List<Jobs> jobs = await SQLCrud.ExecuteModelListAsync<Jobs>(
                    sql,
                    null,
                    reader => new Jobs
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        JobName = reader["JobName"].ToString(),
                        JobTitle = reader["JobTitle"].ToString(),
                        JobYears = reader["JobYears"].ToString(),
                        JobAbout = reader["JobAbout"].ToString(),
                        Status = reader["Status"] != DBNull.Value && Convert.ToBoolean(reader["Status"])
                    },
                    CommandType.StoredProcedure
                );
                var activeJobs = jobs
                    .Where(x => x.Status)
                    .OrderByDescending(x => x.ID) 
                    .ToList();
                return View(activeJobs);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfa İş Hayatı Listeleme İşlemi Hatası", ex.Message);
                return View(new List<Jobs>());
            }
        }
    }
}