using System.Diagnostics;
using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Esportify.Controllers.MVC
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EsportifyContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            EsportifyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Upcoming tournaments (next 7 days) - OK as is
            ViewData["UpcomingTournaments"] = await _context.Tournaments
                .Include(t => t.Game)
                .Where(t => t.StartDate >= DateTime.Now && t.StartDate <= DateTime.Now.AddDays(7))
                .OrderBy(t => t.StartDate)
                .Take(3)
                .ToListAsync();

            // Popular games (most tournaments) - OK as is
            ViewData["PopularGames"] = await _context.Games
                .OrderByDescending(g => g.Tournaments.Count)
                .Take(4)
                .ToListAsync();

            // Tournaments with most registrations - OK as is
            ViewData["PopularTournaments"] = await _context.Tournaments
                .Include(t => t.Game)
                .OrderByDescending(t => t.Registrations.Count)
                .Take(2)
                .ToListAsync();

            // FIXED: Tournaments with highest prize pools
            var tournaments = await _context.Tournaments
                .Include(t => t.Game)
                .Take(100)  // Limit to reasonable number for client-side processing
                .ToListAsync();

            ViewData["HighPrizeTournaments"] = tournaments
                .OrderByDescending(t => t.PrizePool)
                .Take(3);

            if (User.Identity.IsAuthenticated)
            {
                var user = await _context.Users
                    .Include(u => u.FavoriteGames)
                    .ThenInclude(fg => fg.Game)
                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                ViewData["FavoriteGames"] = user?.FavoriteGames.Select(fg => fg.Game).Take(5).ToList();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}