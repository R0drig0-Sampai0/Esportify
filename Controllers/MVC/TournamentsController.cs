using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Esportify.Controllers.MVC
{
    [Authorize]
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
            try
            {
                var tournamentsQuery = _context.Tournaments
                    .Include(t => t.Game)
                    .Include(t => t.Registrations)
                    .Include(t => t.Organizer)
                    .AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    tournamentsQuery = tournamentsQuery.Where(t =>
                        t.Name.Contains(search) ||
                        t.Description.Contains(search));
                }

                // Apply game filter
                if (gameFilter != "all")
                {
                    tournamentsQuery = tournamentsQuery.Where(t => t.Game.Genre.ToLower() == gameFilter.ToLower());
                }

                // Get paginated results
                var totalTournaments = await tournamentsQuery.CountAsync();
                var tournaments = await tournamentsQuery
                    .OrderBy(t => t.StartDate)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                // Get data for filters
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
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, "An error occurred while loading tournaments.");
            }
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

            // Check if current user is registered
            if (User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                ViewBag.IsRegistered = await _context.Registrations
                    .AnyAsync(r => r.TournamentId == id &&
                                  r.Team.TeamMembers.Any(tm => tm.UserId == userId));
            }

            return View(tournament);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewData["GameId"] = new SelectList(await _context.Games.ToListAsync(), "Id", "Name");
                return View();
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, "An error occurred while loading the create form.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,GameId,StartDate,EndDate,RegistrationDeadline,MaxTeams,MinTeamSize,MaxTeamSize,PrizePool,ImageUrl")] Tournament tournament)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    tournament.Id = Guid.NewGuid().ToString();
                    tournament.OrganizerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    tournament.CreatedDate = DateTime.Now;

                    _context.Add(tournament);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                ViewData["GameId"] = new SelectList(await _context.Games.ToListAsync(), "Id", "Name", tournament.GameId);
                return View(tournament);
            }
            catch (Exception ex)
            {
                // Log the error
                ModelState.AddModelError("", "An error occurred while creating the tournament.");
                ViewData["GameId"] = new SelectList(await _context.Games.ToListAsync(), "Id", "Name", tournament.GameId);
                return View(tournament);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }

            ViewData["GameId"] = new SelectList(await _context.Games.ToListAsync(), "Id", "Name", tournament.GameId);
            return View(tournament);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Name,Description,GameId,StartDate,EndDate,RegistrationDeadline,MaxTeams,MinTeamSize,MaxTeamSize,PrizePool,ImageUrl,OrganizerId,CreatedDate")] Tournament tournament)
        {
            if (id != tournament.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tournament);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TournamentExists(tournament.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GameId"] = new SelectList(await _context.Games.ToListAsync(), "Id", "Name", tournament.GameId);
            return View(tournament);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var tournament = await _context.Tournaments
                .Include(t => t.Game)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (tournament == null)
            {
                return NotFound();
            }

            return View(tournament);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament != null)
            {
                _context.Tournaments.Remove(tournament);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string tournamentId, string teamId)
        {
            try
            {
                if (string.IsNullOrEmpty(tournamentId) || string.IsNullOrEmpty(teamId))
                {
                    return BadRequest("Invalid tournament or team");
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isTeamMember = await _context.TeamMembers
                    .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

                if (!isTeamMember)
                {
                    return Forbid();
                }

                var existingRegistration = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.TournamentId == tournamentId && r.TeamId == teamId);

                if (existingRegistration != null)
                {
                    return BadRequest("Team is already registered for this tournament");
                }

                var registration = new Registration
                {
                    TournamentId = tournamentId,
                    TeamId = teamId,
                    RegistrationDate = DateTime.Now
                };

                _context.Registrations.Add(registration);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = tournamentId });
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, "An error occurred while processing your registration.");
            }
        }

        private bool TournamentExists(string id)
        {
            return _context.Tournaments.Any(e => e.Id == id);
        }
    }
}