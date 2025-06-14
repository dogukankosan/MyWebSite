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
                        Icon = reader["Icon"].ToString()
                    },
                    System.Data.CommandType.StoredProcedure
                );
                ViewBag.SelectedIcon = selected; 
                return View(icons);
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("Admin Beceriler Panelde İkon Listesi Çekme Hatası", ex.Message);
                return View(new List<Icons>());
            }
        }
    }
}