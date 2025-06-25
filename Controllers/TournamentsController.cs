using Microsoft.AspNetCore.Mvc;

namespace Esportify.Controllers
{
    public class TournamentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}