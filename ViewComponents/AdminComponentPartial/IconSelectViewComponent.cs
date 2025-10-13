using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.AdminComponentPartial
{
    public class IconSelectViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(string? selected)
        {
            try
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                List<Icons> icons = await SQLCrud.ExecuteModelListAsync(
                    "IconsGetAll",
                    parameters,
                    reader => new Icons
                    {
                        ID = Convert.ToInt32(reader["ID"]),
                        Icon = reader["Icon"].ToString(),
                        Status = Convert.ToBoolean(reader["Status"])
                    },
                    System.Data.CommandType.StoredProcedure
                );
                var activeIcons = icons.Where(x => x.Status).ToList();
                ViewBag.SelectedIcon = selected;
                return View(activeIcons);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Beceriler Panelde İkon Listesi Çekme Hatası", ex.Message);
                return View(new List<Icons>());
            }
        }
    }
}