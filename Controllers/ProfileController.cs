using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Esportify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly EsportifyContext _context;

        public ProfileController(EsportifyContext context)
        {
            _context = context;
        }

        // GET: api/Profile/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserProfile>> GetProfile(int userId)
        {
            var profile = await _context.UserProfiles
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
                return NotFound();

            return profile;
        }

        // POST: api/Profile
        [HttpPost]
        public async Task<ActionResult<UserProfile>> CreateProfile(UserProfile profile)
        {
            if (await _context.UserProfiles.AnyAsync(p => p.UserId == profile.UserId))
                return BadRequest("Profile for this user already exists.");

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProfile), new { userId = profile.UserId }, profile);
        }

        // PUT: api/Profile/5
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateProfile(int userId, UserProfile updatedProfile)
        {
            if (userId != updatedProfile.UserId)
                return BadRequest("User ID mismatch.");

            var profile = await _context.UserProfiles.FindAsync(userId);
            if (profile == null)
                return NotFound();

            // Update fields
            profile.DisplayName = updatedProfile.DisplayName;
            profile.Bio = updatedProfile.Bio;
            profile.AvatarUrl = updatedProfile.AvatarUrl;

            _context.Entry(profile).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Profile/5
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteProfile(Guid userId)
        {
            var profile = await _context.UserProfiles.FindAsync(userId);
            if (profile == null)
                return NotFound();

            _context.UserProfiles.Remove(profile);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
