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

            if (!ModelState.IsValid)
                return View(userInDb);

            var profile = userInDb.Profile ?? new UserProfile { UserId = user.Id };

            profile.DisplayName = user.Profile.DisplayName;
            profile.Bio = user.Profile.Bio;
            profile.Country = user.Profile.Country;
            profile.TwitchUrl = user.Profile.TwitchUrl;
            profile.YouTubeUrl = user.Profile.YouTubeUrl;
            profile.TwitterUrl = user.Profile.TwitterUrl;
            profile.DiscordUrl = user.Profile.DiscordUrl;
            profile.FavoriteGame = user.Profile.FavoriteGame;
            profile.FavoriteTeam = user.Profile.FavoriteTeam;

            // Handle image upload (basic logic)
            if (avatarFile != null && avatarFile.Length > 0)
            {
                var avatarPath = "/uploads/avatars/" + Guid.NewGuid() + Path.GetExtension(avatarFile.FileName);
                using (var stream = new FileStream("wwwroot" + avatarPath, FileMode.Create))
                {
                    await avatarFile.CopyToAsync(stream);
                }
                profile.AvatarUrl = avatarPath;
            }

            if (bannerFile != null && bannerFile.Length > 0)
            {
                var bannerPath = "/uploads/banners/" + Guid.NewGuid() + Path.GetExtension(bannerFile.FileName);
                using (var stream = new FileStream("wwwroot" + bannerPath, FileMode.Create))
                {
                    await bannerFile.CopyToAsync(stream);
                }
                profile.BannerUrl = bannerPath;
            }

            // If profile is new
            if (userInDb.Profile == null)
                _context.UserProfiles.Add(profile);

            await _context.SaveChangesAsync();
            return RedirectToAction("Me");
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

    }
}