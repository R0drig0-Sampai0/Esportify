using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Esportify.Controllers.MVC
{
    public class GamesController : Controller
    {
        private readonly EsportifyContext _context;

        public GamesController(EsportifyContext context)
        {
            _context = context;
        }

        // GET: Lists all games
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 12;
            var games = await _context.Games
                .Include(g => g.Tournaments)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalGames = await _context.Games.CountAsync();
            ViewBag.TotalPages = (int)Math.Ceiling(totalGames / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(games);
        }

        // GET: Shows game details
        [Authorize]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var game = await _context.Games
                .Include(g => g.Tournaments)
                .Include(g => g.LikedByUsers)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: Displays the form to add a new game (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AddGame()
        {
            return View();
        }

        // POST: Adds a new game (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGame(Game game)
        {
            if (ModelState.IsValid)
            {
                // Generate a new ID
                game.Id = Guid.NewGuid().ToString();

                _context.Games.Add(game);
                await _context.SaveChangesAsync();

                // Redirect back to the game list or details page
                return RedirectToAction(nameof(Index));
            }

            // If validation failed, return the form with validation errors
            return View(game);
        }

        // POST: Deletes the game (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGame(string id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}