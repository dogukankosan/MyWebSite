using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class SkillsComponent : ViewComponent
    {
        private readonly IConfiguration _configuration;
        public SkillsComponent(IConfiguration configuration)
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
                    SqlCommand cmd = new("SkillsGet", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    List<Skills> skills = new();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                skills.Add(new Skills()
                                {
                                    ID = Convert.ToByte(reader["ID"]),
                                    SkillName = reader["SkillName"].ToString(),
                                    SkillPercent = Convert.ToByte(reader["SkillPercent"]),
                                    Skillcon = reader["Skillcon"].ToString(),
                                });
                            }
                            return View(skills);
                        }
                    }
                }
                catch (Exception ex)
                {
                    await Logging.LogAdd("Anasayfa Beceriler Listeleme İşlemi Hatası", ex.Message, connectionString, HttpContext.Connection.RemoteIpAddress.ToString());
                }
            }
            return View();
        }
    }
}