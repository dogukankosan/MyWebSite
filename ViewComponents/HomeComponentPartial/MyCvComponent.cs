using Microsoft.AspNetCore.Mvc;
using System.Data;
using MyWebSite.Classes;

namespace MyWebSite.ViewComponents.HomeComponentPartial
{
    public class MyCvComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            byte[] cvData = await GetCvAsync();
            string base64 = cvData != null ? Convert.ToBase64String(cvData) : null;
            return View("Default", base64); 
        }
        private async Task<byte[]> GetCvAsync()
        {
            List<byte[]> result = await SQLCrud.ExecuteModelListAsync(
                "MyCVGet",
                null,
                reader => (byte[])reader["CV"],
                CommandType.StoredProcedure
            );
            return result.FirstOrDefault();
        }
    }
}