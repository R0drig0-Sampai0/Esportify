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

        // GET: Teams/Details/5
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
                        .ThenInclude(t => t.Members)
                                .ThenInclude(m => m.User)
                                    .ThenInclude(u => u.Profile)
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

                var userLeaderTeams = await _context.Teams
                    .Where(t => t.LeaderId == userId)
                    .Include(t => t.Members)
                    .ToListAsync();
                ViewBag.UserTeams = userLeaderTeams;
            }

            return View(tournament);
        }

        // GET: Tournaments/Create
        public IActionResult Create()
        {
            ViewBag.Games = _context.Games.ToList();
            return View();
        }

        // POST: Tournaments/Create
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

        /* GET: Tournament/Edit/5 */
        [Authorize]
        public async Task<IActionResult> Edit(string id)
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tournament.OrganizerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var viewModel = new TournamentsViewModel
            {
                Name = tournament.Name,
                Description = tournament.Description,
                GameId = tournament.GameId,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                RegistrationDeadline = tournament.RegistrationDeadline,
                MaxTeams = tournament.MaxTeams,
                MinTeamSize = tournament.MinTeamSize,
                MaxTeamSize = tournament.MaxTeamSize,
                PrizePool = tournament.PrizePool
            };

            ViewBag.Games = await _context.Games.ToListAsync();
            ViewBag.TournamentId = id;
            ViewBag.CurrentImageUrl = tournament.ImageUrl;

            return View(viewModel);
        }

        // POST: Tournaments/Edit/5
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, TournamentsViewModel model)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tournament.OrganizerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Games = await _context.Games.ToListAsync();
                ViewBag.TournamentId = id;
                ViewBag.CurrentImageUrl = tournament.ImageUrl;
                return View(model);
            }

            try
            {
                tournament.Name = model.Name;
                tournament.Description = model.Description;
                tournament.GameId = model.GameId;
                tournament.StartDate = model.StartDate;
                tournament.EndDate = model.EndDate;
                tournament.RegistrationDeadline = model.RegistrationDeadline;
                tournament.MaxTeams = model.MaxTeams;
                tournament.MinTeamSize = model.MinTeamSize;
                tournament.MaxTeamSize = model.MaxTeamSize;
                tournament.PrizePool = model.PrizePool;

                if (model.Image != null && model.Image.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(model.Image.FileName);
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "tournaments");

                    Directory.CreateDirectory(uploadsPath);

                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(stream);
                    }

                    tournament.ImageUrl = $"/images/tournaments/{fileName}";
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Torneio atualizado com sucesso!";
                return RedirectToAction("Details", new { id = tournament.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro ao atualizar torneio: " + ex.Message;
                ViewBag.Games = await _context.Games.ToListAsync();
                ViewBag.TournamentId = id;
                ViewBag.CurrentImageUrl = tournament.ImageUrl;
                return View(model);
            }
        }

        /* POST: Tournaments/Delete/5 */
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tournament.OrganizerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                _context.Registrations.RemoveRange(tournament.Registrations);

                _context.Tournaments.Remove(tournament);

                await _context.SaveChangesAsync();
                TempData["Success"] = "Torneio eliminado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro ao eliminar torneio: " + ex.Message;
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Register(string tournamentId, string teamId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var team = await _context.Teams
                .FirstOrDefaultAsync(t => t.Id == teamId);

            if (team == null)
            {
                TempData["Error"] = "Equipa não encontrada.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            if (team.LeaderId != userId)
            {
                TempData["Error"] = "Apenas o líder da equipa pode inscrever a equipa em torneios.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var existingRegistration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.TournamentId == tournamentId && r.TeamId == teamId);

            if (existingRegistration != null)
            {
                TempData["Error"] = "Esta equipa já está inscrita neste torneio.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

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

            team = await _context.Teams
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

        /* POST: Tournament/Unregister/55 */
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Unregister(string tournamentId, string teamId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var teamLeader = await _context.Teams
                .Where(t => t.Id == teamId)
                .Select(t => t.LeaderId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(teamLeader))
            {
                TempData["Error"] = "Equipa não encontrada.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            if (teamLeader != userId)
            {
                TempData["Error"] = "Apenas o líder da equipa pode desinscrever a equipa de torneios.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.TournamentId == tournamentId && r.TeamId == teamId);

            if (registration == null)
            {
                TempData["Error"] = "Esta equipa não está inscrita neste torneio.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament != null && tournament.StartDate <= DateTime.Now)
            {
                TempData["Error"] = "Não é possível cancelar a inscrição após o torneio ter iniciado.";
                return RedirectToAction("Details", new { id = tournamentId });
            }

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Inscrição cancelada com sucesso!";
            return RedirectToAction("Details", new { id = tournamentId });
        }
    }
}