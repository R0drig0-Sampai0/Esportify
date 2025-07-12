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
            int pageSize = 12; // Adjust as needed
            var games = await _context.Games
                .Include(g => g.Tournaments)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Add pagination data to ViewBag
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var game = await _context.Games
                .Include(g => g.Tournaments)
                .Include(g => g.LikedByUsers)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (game == null)
            {
                return NotFound();
            }

            ViewData["IsLiked"] = userId != null && game.LikedByUsers.Any(ug => ug.UserId == userId);
            return View(game);
        }

        // GET: Lists games liked by the user
        [Authorize]
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

        // GET: Displays the form to add a new game (Admin only)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult AddGame()
        {
            return View();
        }

        // POST: Handles the form submission to add a new game
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGame(Game model, IFormFile imageFile)
        {
            // Debug: Log the incoming model data
            Console.WriteLine($"Model received - Name: {model.Name}, Genre: {model.Genre}, OfficialWebsite: {model.OfficialWebsite}");

            // Check ModelState
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                Console.WriteLine("ModelState Errors: " + string.Join("; ", errors));
                return View(model);
            }

            try
            {
                string imagePath = "/images/default-game.jpg";

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    Console.WriteLine("Image file detected, processing upload...");
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    imagePath = "/images/" + uniqueFileName;
                    Console.WriteLine($"Image saved at: {imagePath}");
                }

                // Set model properties
                model.Id = Guid.NewGuid().ToString();
                model.ImageUrl = imagePath;
                model.OfficialWebsite = string.IsNullOrEmpty(model.OfficialWebsite) ? "#" : model.OfficialWebsite;
                model.Genre = model.Genre ?? "Other";

                // Debug: Log before saving
                Console.WriteLine($"Saving game - Id: {model.Id}, Name: {model.Name}, ImageUrl: {model.ImageUrl}");

                // Add to context and save
                _context.Games.Add(model);
                int rowsAffected = await _context.SaveChangesAsync();
                Console.WriteLine($"SaveChangesAsync completed, rows affected: {rowsAffected}");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during save: {ex.Message}\nStackTrace: {ex.StackTrace}");
                ModelState.AddModelError("", "Ocorreu um erro ao salvar o jogo. Verifique o console para detalhes.");
                return View(model);
            }
        }
    }
}