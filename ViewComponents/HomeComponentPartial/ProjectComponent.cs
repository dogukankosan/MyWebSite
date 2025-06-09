using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class ProjectComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                string sql = "ProjectsGet";
                List<SqlParameter> parameters = new List<SqlParameter>();
                List<Projects> projects = await SQLCrud.ExecuteModelListAsync<Projects>(
                    sql,
                    parameters,
                    reader =>
                    {
                        string base64Image = "";
                        if (!Convert.IsDBNull(reader["ProjectImg"]))
                        {
                            byte[] imageBytes = (byte[])reader["ProjectImg"];
                            base64Image = Convert.ToBase64String(imageBytes);
                        }
                        return new Projects
                        {
                            ID = Convert.ToByte(reader["ID"]),
                            ProjectName = reader["ProjectName"].ToString(),
                            Base64Pictures = base64Image,
                            ProjectDescription = reader["ProjectDescription"].ToString(),
                            ProjectGithubLink = reader["ProjectGithubLink"].ToString(),
                            ProjectLink = reader["ProjectLink"].ToString()
                        };
                    },
                    System.Data.CommandType.StoredProcedure
                );
                return View(projects);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfa Projeler Listeleme İşlemi Hatası", ex.Message);
                return View(new List<Projects>());
            }
        }
    }
}