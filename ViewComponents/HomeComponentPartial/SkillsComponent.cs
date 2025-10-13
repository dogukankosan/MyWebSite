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
                const string sql = "SkillsGet";
                var parameters = new List<SqlParameter>();
                var allSkills = await SQLCrud.ExecuteModelListAsync<Skills>(
                    sql,
                    parameters,
                    reader => new Skills
                    {
                        ID = Convert.ToByte(reader["ID"]),
                        SkillName = reader["SkillName"]?.ToString(),
                        SkillPercent = reader["SkillPercent"] == DBNull.Value
                            ? (byte)0
                            : Convert.ToByte(reader["SkillPercent"]),
                        Skillcon = reader["Skillcon"]?.ToString(),
                        Status = reader["Status"] != DBNull.Value && Convert.ToBoolean(reader["Status"])
                    },
                    CommandType.StoredProcedure
                );
                var activeSkills = allSkills
                    .Where(s => s.Status)
                    .OrderByDescending(s => s.SkillPercent)
                    .ThenBy(s => s.SkillName)              
                    .ToList();
                return View(activeSkills);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Anasayfa Beceriler Listeleme İşlemi Hatası", ex.Message);
                return View(new List<Skills>());
            }
        }
    }
}