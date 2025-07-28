using Microsoft.AspNetCore.Mvc;

namespace EmreGaleriApp.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ErrorController : Controller
    {
        [HttpGet]
        public IActionResult AccessDenied()
        {


            return View();
        }
    }
}
