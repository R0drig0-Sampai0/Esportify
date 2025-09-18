using Esportify.Data;
using Esportify.Models;
using Esportify.Models.ViewModels;
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
                                  r.Team.Members.Any(tm => tm.UserId == userId));
            }

            return View(tournament);
        }

        public IActionResult Create()
        {
            ViewBag.Games = _context.Games.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TournamentsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Games = _context.Games.ToList();
                return View(model);
            }

            string imageUrl = "/images/tournaments/default.jpg";

            if (model.Image != null && model.Image.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.Image.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/tournaments", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                imageUrl = "/images/tournaments/" + fileName;
            }

            var tournament = new Tournament
            {
                Id = Guid.NewGuid().ToString(),
                Name = model.Name,
                Description = model.Description,
                GameId = model.GameId,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                RegistrationDeadline = model.RegistrationDeadline,
                MaxTeams = model.MaxTeams,
                MinTeamSize = model.MinTeamSize,
                MaxTeamSize = model.MaxTeamSize,
                PrizePool = model.PrizePool,
                ImageUrl = imageUrl,
                OrganizerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                CreatedDate = DateTime.UtcNow
            };

            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Register(string tournamentId, string teamId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Check if user is a member of the team
            var isTeamMember = await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

            if (!isTeamMember)
            {
                TempData["Error"] = "Não tens permissão para inscrever esta equipa.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            // Check if team is already registered
            var existingRegistration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.TournamentId == tournamentId && r.TeamId == teamId);

            if (existingRegistration != null)
            {
                TempData["Error"] = "Esta equipa já está inscrita neste torneio.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            // Check if tournament is still accepting registrations
            var tournament = await _context.Tournaments
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);
            if (tournament == null)
            {
                TempData["Error"] = "Torneio não encontrado.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            if (tournament.RegistrationDeadline < DateTime.Now)
            {
                TempData["Error"] = "O prazo de inscrição já expirou.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var registrationCount = tournament.Registrations?.Count ?? 0;
            if (registrationCount >= tournament.MaxTeams)
            {
                TempData["Error"] = "Este torneio já atingiu o número máximo de equipas.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            // Check team size requirements
            var team = await _context.Teams
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
            {
                TempData["Error"] = "Equipa não encontrada.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            if (team.Members.Count < tournament.MinTeamSize || team.Members.Count > tournament.MaxTeamSize)
            {
                TempData["Error"] = $"A equipa deve ter entre {tournament.MinTeamSize} e {tournament.MaxTeamSize} membros.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            // Create registration
            var registration = new Registration
            {
                TournamentId = tournamentId,
                TeamId = teamId,
                RegistrationDate = DateTime.Now
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Equipa inscrita com sucesso no torneio!";
            return RedirectToAction("Details", new { id = tournamentId });
        }
    }
}