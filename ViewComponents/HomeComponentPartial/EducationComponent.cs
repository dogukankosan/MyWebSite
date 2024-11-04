using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class EducationComponent : ViewComponent
    {
        private readonly IConfiguration _configuration;
        public EducationComponent(IConfiguration configuration)
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
                    SqlCommand cmd = new("EducationGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<Education> educations = new();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                educations.Add(new Education()
                                {
                                    ID = Convert.ToByte(reader["ID"]),
                                    SchoolName = reader["SchoolName"].ToString(),
                                    SectionName = reader["SectionName"].ToString(),
                                    Years = reader["Years"].ToString(),
                                });
                            }
                            return View(educations);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Anasayfada Eğitim Listeleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                }
            }
            return View();
        }
    }
}