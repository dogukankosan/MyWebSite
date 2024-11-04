using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class JobComponent : ViewComponent
    {
        private readonly IConfiguration _configuration;
        public JobComponent(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("JobsGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<Jobs> jobs = new();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                jobs.Add(new Jobs()
                                {
                                    ID = Convert.ToByte(reader["ID"]),
                                    JobName = reader["JobName"].ToString(),
                                    JobTitle = reader["JobTitle"].ToString(),
                                    JobYears = reader["JobYears"].ToString(),
                                    JobAbout = reader["JobAbout"].ToString(),
                                });
                            }
                            return View(jobs);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Anasayfa Eğitim Listeleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                }
            }
            return View();
        }
    }
}