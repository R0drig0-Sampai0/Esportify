using Microsoft.AspNetCore.Mvc;

namespace Esportify.Controllers
{
    public class GamesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
