using Microsoft.AspNetCore.Mvc;

namespace Esportify.Controllers.MVC
{
    public class DeveloperController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
