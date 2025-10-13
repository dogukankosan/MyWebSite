using Microsoft.AspNetCore.Mvc;
using MyWebSite.Classes;
using MyWebSite.Models;
using System.Data;
using System.Data.SqlClient;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class WhatsAppSupportComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            string phone = "";
            try
            {
                string sql = "AboutGet";
                List<About> aboutList = await SQLCrud.ExecuteModelListAsync(
                    sql,
                    null,
                    reader => new About
                    {
                        AboutPhone = reader["AboutPhone"]?.ToString()
                    },
                    CommandType.StoredProcedure
                );
                phone = aboutList.FirstOrDefault()?.AboutPhone ?? "";
            }
            catch (Exception ex)
            {
                await Logging.LogAdd("WhatsApp Component Telefon Alma Hatası", ex.Message);
            }
            return View("Default", phone);
        }
    }
}