using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
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
                List<Projects> projects = await SQLCrud.ExecuteModelListAsync<Projects>(
                    sql,
                    null,
                    reader =>
                    {
                        string base64Image = string.Empty;
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
                            ProjectLink = reader["ProjectLink"].ToString(),
                            Status = reader["Status"] != DBNull.Value && Convert.ToBoolean(reader["Status"])
                        };
                    },
                    CommandType.StoredProcedure
                );
                var activeProjects = projects
                    .Where(x => x.Status)
                    .OrderByDescending(x => x.ID) 
                    .ToList();
                return View(activeProjects);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfa Projeler Listeleme İşlemi Hatası", ex.Message);
                return View(new List<Projects>());
            }
        }
    }
}