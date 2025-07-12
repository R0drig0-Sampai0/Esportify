using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Esportify.Controllers
{
    public class LandingPageController(EsportifyContext context) : Controller
    {
        private readonly EsportifyContext _context = context;

        public async Task<IActionResult> Index()
        {
            var model = new LandingPageViewModel
            {
                FeaturedGames = await _context.Games
                    .Take(4)
                    .Select(g => new Game
                    {
                        Id = g.Id,
                        Name = g.Name,
                        ImageUrl = g.ImageUrl
                    })
                    .ToListAsync(),

                TopPlayers = await _context.Users
                    .Join(_context.UserProfiles,
                        user => user.Id,
                        profile => profile.UserId,
                        (user, profile) => new { user, profile })
                    .OrderByDescending(up => (double)up.profile.Earnings) // Cast decimal to double for SQLite
                    .Take(3)
                    .Select(up => new PlayerViewModel
                    {
                        UserName = up.user.UserName,
                        AvatarUrl = up.profile.AvatarUrl ?? "/images/default-avatar.jpg",
                        Earnings = up.profile.Earnings
                    })
                    .ToListAsync(),

                PlayerCount = await _context.Users.CountAsync(),
                TournamentCount = await _context.Tournaments.CountAsync()
            };

            return View(model);
        }
    }
}