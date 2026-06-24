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
            // Upcoming tournaments (next 7 days)
            var upcomingTournamentRows = await _context.Tournaments
                .Where(t => t.StartDate >= DateTime.Now && t.StartDate <= DateTime.Now.AddDays(7))
                .OrderBy(t => t.StartDate)
                .Take(3)
                .Select(t => new TournamentCardRow
                {
                    Id = t.Id ?? string.Empty,
                    Name = t.Name ?? string.Empty,
                    Description = t.Description ?? string.Empty,
                    ImageUrl = t.ImageUrl ?? string.Empty,
                    StartDate = (DateTime?)t.StartDate,
                    EndDate = (DateTime?)t.EndDate,
                    RegistrationDeadline = (DateTime?)t.RegistrationDeadline,
                    MaxTeams = (int?)t.MaxTeams,
                    PrizePool = (decimal?)t.PrizePool,
                    RegistrationsCount = t.Registrations.Count(),
                    HasGame = t.Game != null,
                    GameId = t.Game != null ? t.Game.Id ?? string.Empty : string.Empty,
                    GameName = t.Game != null ? t.Game.Name ?? string.Empty : string.Empty,
                    GameImageUrl = t.Game != null ? t.Game.ImageUrl ?? string.Empty : string.Empty,
                    GameCreatedAt = t.Game != null ? (DateTime?)t.Game.CreatedAt : null
                })
                .ToListAsync();

            ViewData["UpcomingTournaments"] = upcomingTournamentRows
                .Select(MapTournamentCard)
                .ToList();

            // Popular games (most tournaments) 
            var popularGameRows = await _context.Games
                .OrderByDescending(g => g.Tournaments.Count)
                .Take(4)
                .Select(g => new GameCardRow
                {
                    Id = g.Id ?? string.Empty,
                    Name = g.Name ?? string.Empty,
                    ImageUrl = g.ImageUrl ?? string.Empty,
                    TournamentsCount = g.Tournaments.Count()
                })
                .ToListAsync();

            ViewData["PopularGames"] = popularGameRows
                .Select(MapGameCard)
                .ToList();

            // Tournaments with most registrations 
            var popularTournamentRows = await _context.Tournaments
                .OrderByDescending(t => t.Registrations.Count)
                .Take(2)
                .Select(t => new TournamentCardRow
                {
                    Id = t.Id ?? string.Empty,
                    Name = t.Name ?? string.Empty,
                    Description = t.Description ?? string.Empty,
                    ImageUrl = t.ImageUrl ?? string.Empty,
                    StartDate = (DateTime?)t.StartDate,
                    EndDate = (DateTime?)t.EndDate,
                    RegistrationDeadline = (DateTime?)t.RegistrationDeadline,
                    MaxTeams = (int?)t.MaxTeams,
                    PrizePool = (decimal?)t.PrizePool,
                    RegistrationsCount = t.Registrations.Count(),
                    HasGame = t.Game != null,
                    GameId = t.Game != null ? t.Game.Id ?? string.Empty : string.Empty,
                    GameName = t.Game != null ? t.Game.Name ?? string.Empty : string.Empty,
                    GameImageUrl = t.Game != null ? t.Game.ImageUrl ?? string.Empty : string.Empty,
                    GameCreatedAt = t.Game != null ? (DateTime?)t.Game.CreatedAt : null
                })
                .ToListAsync();

            ViewData["PopularTournaments"] = popularTournamentRows
                .Select(MapTournamentCard)
                .ToList();

            var tournamentRows = await _context.Tournaments
                .Take(100)
                .Select(t => new TournamentCardRow
                {
                    Id = t.Id ?? string.Empty,
                    Name = t.Name ?? string.Empty,
                    Description = t.Description ?? string.Empty,
                    ImageUrl = t.ImageUrl ?? string.Empty,
                    StartDate = (DateTime?)t.StartDate,
                    EndDate = (DateTime?)t.EndDate,
                    RegistrationDeadline = (DateTime?)t.RegistrationDeadline,
                    MaxTeams = (int?)t.MaxTeams,
                    PrizePool = (decimal?)t.PrizePool,
                    RegistrationsCount = t.Registrations.Count(),
                    HasGame = t.Game != null,
                    GameId = t.Game != null ? t.Game.Id ?? string.Empty : string.Empty,
                    GameName = t.Game != null ? t.Game.Name ?? string.Empty : string.Empty,
                    GameImageUrl = t.Game != null ? t.Game.ImageUrl ?? string.Empty : string.Empty,
                    GameCreatedAt = t.Game != null ? (DateTime?)t.Game.CreatedAt : null
                })
                .ToListAsync();

            ViewData["HighPrizeTournaments"] = tournamentRows
                .Select(MapTournamentCard)
                .OrderByDescending(t => t.PrizePool)
                .Take(3);

            if (User.Identity.IsAuthenticated)
            {
                var favoriteGameRows = await _context.UserGames
                    .Where(fg => fg.User.UserName == User.Identity.Name && fg.Game != null)
                    .Select(fg => new GameCardRow
                    {
                        Id = fg.Game.Id ?? string.Empty,
                        Name = fg.Game.Name ?? string.Empty,
                        ImageUrl = fg.Game.ImageUrl ?? string.Empty,
                        TournamentsCount = fg.Game.Tournaments.Count()
                    })
                    .ToListAsync();

                ViewData["FavoriteGames"] = favoriteGameRows.Select(MapGameCard).ToList();
            }

            return View();
        }

        private static Tournament MapTournamentCard(TournamentCardRow row)
        {
            return new Tournament
            {
                Id = row.Id ?? string.Empty,
                Name = row.Name ?? string.Empty,
                Description = row.Description ?? string.Empty,
                ImageUrl = string.IsNullOrWhiteSpace(row.ImageUrl)
                    ? "/images/tournaments/default.png"
                    : row.ImageUrl,
                StartDate = row.StartDate ?? default,
                EndDate = row.EndDate ?? default,
                RegistrationDeadline = row.RegistrationDeadline ?? default,
                MaxTeams = row.MaxTeams ?? 0,
                PrizePool = row.PrizePool ?? 0,
                GameId = row.GameId ?? string.Empty,
                Game = !row.HasGame
                    ? null
                    : new Game
                    {
                        Id = row.GameId ?? string.Empty,
                        Name = row.GameName ?? string.Empty,
                        ImageUrl = string.IsNullOrWhiteSpace(row.GameImageUrl)
                            ? "/images/games/default.jpg"
                            : row.GameImageUrl,
                        CreatedAt = row.GameCreatedAt ?? default
                    },
                Registrations = Enumerable.Range(0, row.RegistrationsCount)
                    .Select(_ => new Registration())
                    .ToList()
            };
        }

        private static Game MapGameCard(GameCardRow row)
        {
            return new Game
            {
                Id = row.Id ?? string.Empty,
                Name = row.Name ?? string.Empty,
                ImageUrl = string.IsNullOrWhiteSpace(row.ImageUrl)
                    ? "/images/games/default.jpg"
                    : row.ImageUrl,
                Tournaments = Enumerable.Range(0, row.TournamentsCount)
                    .Select(_ => new Tournament())
                    .ToList()
            };
        }

        private sealed class TournamentCardRow
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? Description { get; set; }
            public string? ImageUrl { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public DateTime? RegistrationDeadline { get; set; }
            public int? MaxTeams { get; set; }
            public decimal? PrizePool { get; set; }
            public int RegistrationsCount { get; set; }
            public bool HasGame { get; set; }
            public string? GameId { get; set; }
            public string? GameName { get; set; }
            public string? GameImageUrl { get; set; }
            public DateTime? GameCreatedAt { get; set; }
        }

        private sealed class GameCardRow
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? ImageUrl { get; set; }
            public int TournamentsCount { get; set; }
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
