using Esportify.Data;
using Esportify.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;

namespace Esportify.Controllers.API
{
    [Route("api/games")]
    [ApiController]
    public class GamesApiController : ControllerBase
    {
        private readonly EsportifyContext _context;
        private readonly IAntiforgery _antiforgery;

        public GamesApiController(EsportifyContext context, IAntiforgery antiforgery)
        {
            _context = context;
            _antiforgery = antiforgery;
        }

        // POST api/games/{gameId}/toggle-like
        [HttpPost("{gameId}/toggle-like")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var game = await _context.Games
                .Include(g => g.LikedByUsers)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null)
            {
                return NotFound(new { error = "Game not found" });
            }

            var existingLike = await _context.UserGames
                .FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);

            if (existingLike != null)
            {
                _context.UserGames.Remove(existingLike);
            }
            else
            {
                _context.UserGames.Add(new UserGame
                {
                    UserId = userId,
                    GameId = gameId
                });
            }

            await _context.SaveChangesAsync();

            var newLikeCount = await _context.UserGames
                .CountAsync(ug => ug.GameId == gameId);

            return Ok(new
            {
                isLiked = existingLike == null,
                likeCount = newLikeCount
            });
        }
    }
}