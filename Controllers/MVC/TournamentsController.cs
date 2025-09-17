using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Esportify.Controllers.MVC
{
    public class TournamentsController : Controller
    {
        private readonly EsportifyContext _context;
        private const int PageSize = 12;

        public TournamentsController(EsportifyContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(int page = 1, string search = "", string gameFilter = "all")
        {
            var tournamentsQuery = _context.Tournaments
                .Include(t => t.Game)
                .Include(t => t.Registrations)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                tournamentsQuery = tournamentsQuery.Where(t =>
                    t.Name.Contains(search) ||
                    t.Description.Contains(search));
            }

            if (gameFilter != "all")
            {
                tournamentsQuery = tournamentsQuery.Where(t => t.Game.Genre.ToLower() == gameFilter.ToLower());
            }

            var totalTournaments = await tournamentsQuery.CountAsync();
            var tournaments = await tournamentsQuery
                .OrderBy(t => t.StartDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var gameGenres = await _context.Games
                .Select(g => g.Genre)
                .Distinct()
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalTournaments / (double)PageSize);
            ViewBag.SearchTerm = search;
            ViewBag.GameFilter = gameFilter;
            ViewBag.GameGenres = gameGenres;

            return View(tournaments);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments
                .Include(t => t.Game)
                .Include(t => t.Organizer)
                .Include(t => t.Registrations)
                    .ThenInclude(r => r.Team)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.IsRegistered = await _context.Registrations
                    .AnyAsync(r => r.TournamentId == id &&
                                  r.Team.TeamMembers.Any(tm => tm.UserId == userId));
            }

            return View(tournament);
        }
    }
}