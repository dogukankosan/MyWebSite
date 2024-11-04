using Microsoft.AspNetCore.Mvc;
using MyWebSite.Models;
namespace MyWebSite.ViewComponents.BlogsUserCommentsComponentPartial
{
    public class ContactUserComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            Contacts cs = new();
            return View(cs);
        }
    }
}