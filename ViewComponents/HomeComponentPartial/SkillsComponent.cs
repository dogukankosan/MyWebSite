using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class SkillsComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                string sql = "SkillsGet";
                List<SqlParameter> parameters = new List<SqlParameter>();
                List<Skills> skills = await SQLCrud.ExecuteModelListAsync<Skills>(
                    sql,
                    parameters,
                    reader => new Skills
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        SkillName = reader["SkillName"].ToString(),
                        SkillPercent = Convert.ToByte(reader["SkillPercent"]),
                        Skillcon = reader["Skillcon"].ToString()
                    },
                    CommandType.StoredProcedure
                );
                return View(skills);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfa Beceriler Listeleme İşlemi Hatası", ex.Message);
                return View(new List<Skills>());
            }
        }
    }
}