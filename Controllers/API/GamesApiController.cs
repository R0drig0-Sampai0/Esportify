using Esportify.Data;
using Esportify.Models;
using Esportify.DTOs;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;

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

        [HttpGet]
        public async Task<IActionResult> GetGames()
        {
            var games = await _context.Games
                .Select(g => new GameDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Genre = g.Genre,
                    ImageUrl = g.ImageUrl,
                    OfficialWebsite = g.OfficialWebsite,
                    CreatedAt = g.CreatedAt
                })
                .ToListAsync();

            return Ok(games);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGame(string id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null) return NotFound();

            var dto = new GameDto
            {
                Id = game.Id,
                Name = game.Name,
                Genre = game.Genre,
                ImageUrl = game.ImageUrl,
                OfficialWebsite = game.OfficialWebsite,
                CreatedAt = game.CreatedAt
            };

            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateGame([FromBody] CreateGameDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var game = new Game
            {
                Id = Guid.NewGuid().ToString(),
                Name = createDto.Name,
                Genre = createDto.Genre,
                ImageUrl = string.IsNullOrWhiteSpace(createDto.ImageUrl) ? "/images/games/default.jpg" : createDto.ImageUrl,
                OfficialWebsite = createDto.OfficialWebsite,
                CreatedAt = DateTime.UtcNow
            };

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            var dto = new GameDto
            {
                Id = game.Id,
                Name = game.Name,
                Genre = game.Genre,
                ImageUrl = game.ImageUrl,
                OfficialWebsite = game.OfficialWebsite,
                CreatedAt = game.CreatedAt
            };

            return CreatedAtAction(nameof(GetGame), new { id = game.Id }, dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGame(string id, [FromBody] UpdateGameDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var game = await _context.Games.FindAsync(id);
            if (game == null) return NotFound();

            game.Name = updateDto.Name;
            game.Genre = updateDto.Genre;
            game.OfficialWebsite = updateDto.OfficialWebsite;
            if (!string.IsNullOrWhiteSpace(updateDto.ImageUrl))
            {
                game.ImageUrl = updateDto.ImageUrl;
            }

            _context.Games.Update(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteGame(string id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null) return NotFound();

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("like/{gameId}")]
        [Authorize]
        public async Task<IActionResult> LikeGame(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var exists = await _context.UserGames.AnyAsync(ug => ug.UserId == userId && ug.GameId == gameId);
            if (exists) return BadRequest(new { message = "Already liked" });

            _context.UserGames.Add(new UserGame { UserId = userId, GameId = gameId });
            await _context.SaveChangesAsync();

            return Ok(new { message = "Game liked!" });
        }

        [HttpPost("unlike/{gameId}")]
        [Authorize]
        public async Task<IActionResult> UnlikeGame(string gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var userGame = await _context.UserGames.FirstOrDefaultAsync(ug => ug.UserId == userId && ug.GameId == gameId);
            if (userGame == null) return BadRequest(new { message = "Game not liked yet" });

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
