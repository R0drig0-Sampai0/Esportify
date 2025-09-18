using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Esportify.Controllers.MVC
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly EsportifyContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            EsportifyContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Profile/Me
        public async Task<IActionResult> Me()
        {
            var currentUser = await GetCurrentUserWithProfileAsync();
            if (currentUser == null)
            {
                return NotFound();
            }

            return View(currentUser);
        }

        // GET: Profile/Edit
        public async Task<IActionResult> Edit()
        {
            var currentUser = await GetCurrentUserWithProfileAsync();
            if (currentUser == null)
            {
                return NotFound();
            }

            // Load dropdown data
            await LoadViewBagDataAsync();

            return View(currentUser);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user, IFormFile avatarFile, IFormFile bannerFile)
        {
            var userInDb = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (userInDb == null) return NotFound();

            // Ensure user can only edit their own profile
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userInDb.Id != currentUserId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                // Reload ViewBag data for dropdowns
                await LoadViewBagDataAsync();
                return View(userInDb);
            }

            var profile = userInDb.Profile ?? new UserProfile { UserId = user.Id };

            // Update profile fields
            profile.DisplayName = user.Profile?.DisplayName;
            profile.Bio = user.Profile?.Bio;
            profile.Country = user.Profile?.Country;
            profile.TwitchUrl = user.Profile?.TwitchUrl;
            profile.YouTubeUrl = user.Profile?.YouTubeUrl;
            profile.TwitterUrl = user.Profile?.TwitterUrl;
            profile.DiscordUrl = user.Profile?.DiscordUrl;
            profile.FavoriteGame = user.Profile?.FavoriteGame;
            profile.FavoriteTeam = user.Profile?.FavoriteTeam;

            try
            {
                // Handle avatar upload
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
                    var avatarsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile");
                    
                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(avatarsPath);
                    
                    var filePath = Path.Combine(avatarsPath, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }
                    
                    profile.AvatarUrl = $"/images/profile/{fileName}";
                }

                // Handle banner upload
                if (bannerFile != null && bannerFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(bannerFile.FileName);
                    var bannersPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profile");
                    
                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(bannersPath);
                    
                    var filePath = Path.Combine(bannersPath, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await bannerFile.CopyToAsync(stream);
                    }
                    
                    profile.BannerUrl = $"/images/profile/{fileName}";
                }

                // If profile is new, add it to context
                if (userInDb.Profile == null)
                {
                    _context.UserProfiles.Add(profile);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Perfil atualizado com sucesso!";
                return RedirectToAction("Me");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro ao salvar o perfil: " + ex.Message;
                // Reload ViewBag data for dropdowns
                await LoadViewBagDataAsync();
                return View(userInDb);
            }
        }

        // GET: Profile/{username}
        [AllowAnonymous]
        public async Task<IActionResult> Index(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        private async Task<User?> GetCurrentUserWithProfileAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return null;

            return await _context.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        private async Task LoadViewBagDataAsync()
        {
            // Get all games for favorite game dropdown
            var games = await _context.Games
                .OrderBy(g => g.Name)
                .ToListAsync();
            ViewBag.Games = games;

            // Get teams that the user is a member of
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userTeams = await _context.TeamMembers
                .Where(tm => tm.UserId == userId)
                .Include(tm => tm.Team)
                .Select(tm => tm.Team)
                .OrderBy(t => t.Name)
                .ToListAsync();
            ViewBag.UserTeams = userTeams;
        }

    }
}