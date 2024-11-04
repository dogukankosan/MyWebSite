using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class ProjectComponent : ViewComponent
    {
        private readonly IConfiguration _configuration;
        public ProjectComponent(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Projects> projects = new();
            string base64Image1 = "";
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection con = new(connectionString))
            {
                try
                {
                    await con.OpenAsync();
                    SqlCommand cmd = new("ProjectsGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (!Convert.IsDBNull(reader["ProjectImg"]))
                                {
                                    byte[] imageBytes = (byte[])reader["ProjectImg"];
                                    base64Image1 = Convert.ToBase64String(imageBytes);
                                }
                                projects.Add(new Projects()
                                {
                                    ID = Convert.ToByte(reader["ID"]),
                                    ProjectName = reader["ProjectName"].ToString(),
                                    Base64Pictures = base64Image1,
                                    ProjectDescription = reader["ProjectDescription"].ToString(),
                                    ProjectGithubLink = reader["ProjectGithubLink"].ToString(),
                                    ProjectLink = reader["ProjectLink"].ToString()
                                });
                            }
                            return View(projects);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Anasayfa Projeler Listeleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                }
            }
            return View(projects);
        }
    }
}