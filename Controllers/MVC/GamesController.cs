using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Esportify.Controllers.MVC
{
    public class GamesController : Controller
    {
        private readonly EsportifyContext _context;

        public GamesController(EsportifyContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var games = await _context.Games
                .Include(g => g.Tournaments)
                .ToListAsync();

            return View(games);
        }

        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var game = await _context.Games
                .Include(g => g.Tournaments)
                .Include(g => g.LikedByUsers)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            // Pass whether the current user has liked the game
            ViewData["IsLiked"] = userId != null && game.LikedByUsers.Any(ug => ug.UserId == userId);

            return View(game);
        }

        public async Task<IActionResult> LikedGames()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var likedGames = await _context.UserGames
                .Where(ug => ug.UserId == userId)
                .Include(ug => ug.Game)
                    .ThenInclude(g => g.Tournaments)
                .Select(ug => ug.Game)
                .ToListAsync();

            return View(likedGames);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AddGame()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddGame(Game model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Id = Guid.NewGuid().ToString();
            model.ImageUrl = string.IsNullOrEmpty(model.ImageUrl) ? "/images/default-game.jpg" : model.ImageUrl;
            model.OfficialWebsite = string.IsNullOrEmpty(model.OfficialWebsite) ? "#" : model.OfficialWebsite;
            model.Genre = model.Genre ?? "Other";

            _context.Games.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}