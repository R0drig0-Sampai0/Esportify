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

        public GamesApiController(EsportifyContext context)
        {
            _context = context;
        }

        [HttpPost("like/{gameId}")]
        public async Task<IActionResult> LikeGame(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var exists = await _context.UserGames.AnyAsync(ug => ug.UserId == userId && ug.GameId == gameId);
            if (exists) return BadRequest("Already liked");

            _context.UserGames.Add(new UserGame { UserId = userId, GameId = gameId });
            await _context.SaveChangesAsync();

            return Ok(new { message = "Game liked!" });
        }

        [HttpPost("unlike/{gameId}")]
        public async Task<IActionResult> UnlikeGame(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var userGame = await _context.UserGames.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);
            if (userGame == null) return BadRequest("Game not liked yet");

            _context.UserGames.Remove(userGame);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Game unliked" });
        }

        [HttpGet("{gameId}/likes")]
        public async Task<IActionResult> GetLikes(string gameId)
        {
            var count = await _context.UserGames.CountAsync(ug => ug.GameId == gameId);
            return Ok(new { likes = count });
        }

    }
}