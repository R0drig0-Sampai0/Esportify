using Esportify.Data;
using Esportify.DTOs;
using Esportify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System;
using System.Linq;

namespace Esportify.Controllers.API
{
    [Route("api/tournaments")]
    [ApiController]
    [Authorize]
    public class TournamentsApiController : ControllerBase
    {
        private readonly EsportifyContext _context;

        public TournamentsApiController(EsportifyContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTournaments()
        {
            var tournaments = await _context.Tournaments
                .Select(t => new TournamentDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    GameId = t.GameId,
                    Game = new GameDto
                    {
                        Id = t.Game.Id,
                        Name = t.Game.Name,
                        Genre = t.Game.Genre,
                        ImageUrl = t.Game.ImageUrl,
                        OfficialWebsite = t.Game.OfficialWebsite,
                        CreatedAt = t.Game.CreatedAt
                    },
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    RegistrationDeadline = t.RegistrationDeadline,
                    MaxTeams = t.MaxTeams,
                    MinTeamSize = t.MinTeamSize,
                    MaxTeamSize = t.MaxTeamSize,
                    PrizePool = t.PrizePool,
                    ImageUrl = t.ImageUrl,
                    OrganizerId = t.OrganizerId,
                    OrganizerName = t.Organizer != null ? (t.Organizer.Profile != null ? t.Organizer.Profile.DisplayName ?? t.Organizer.UserName : t.Organizer.UserName) : null,
                    CreatedDate = t.CreatedDate,
                    RegisteredTeamsCount = t.Registrations.Count()
                })
                .ToListAsync();

            return Ok(tournaments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTournament(string id)
        {
            var tournament = await _context.Tournaments
                .Where(t => t.Id == id)
                .Select(t => new TournamentDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    GameId = t.GameId,
                    Game = new GameDto
                    {
                        Id = t.Game.Id,
                        Name = t.Game.Name,
                        Genre = t.Game.Genre,
                        ImageUrl = t.Game.ImageUrl,
                        OfficialWebsite = t.Game.OfficialWebsite,
                        CreatedAt = t.Game.CreatedAt
                    },
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    RegistrationDeadline = t.RegistrationDeadline,
                    MaxTeams = t.MaxTeams,
                    MinTeamSize = t.MinTeamSize,
                    MaxTeamSize = t.MaxTeamSize,
                    PrizePool = t.PrizePool,
                    ImageUrl = t.ImageUrl,
                    OrganizerId = t.OrganizerId,
                    OrganizerName = t.Organizer != null ? (t.Organizer.Profile != null ? t.Organizer.Profile.DisplayName ?? t.Organizer.UserName : t.Organizer.UserName) : null,
                    CreatedDate = t.CreatedDate,
                    RegisteredTeamsCount = t.Registrations.Count()
                })
                .FirstOrDefaultAsync();

            if (tournament == null) return NotFound();

            return Ok(tournament);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTournament([FromBody] CreateTournamentDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var gameExists = await _context.Games.AnyAsync(g => g.Id == createDto.GameId);
            if (!gameExists) return BadRequest("O jogo indicado não existe.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tournament = new Tournament
            {
                Id = Guid.NewGuid().ToString(),
                Name = createDto.Name,
                Description = createDto.Description,
                GameId = createDto.GameId,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                RegistrationDeadline = createDto.RegistrationDeadline ?? createDto.StartDate,
                MaxTeams = createDto.MaxTeams,
                MinTeamSize = createDto.MinTeamSize,
                MaxTeamSize = createDto.MaxTeamSize,
                PrizePool = createDto.PrizePool,
                ImageUrl = string.IsNullOrWhiteSpace(createDto.ImageUrl) ? "/images/tournaments/default.png" : createDto.ImageUrl,
                OrganizerId = userId ?? createDto.OrganizerId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();

            var dto = new TournamentDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                Description = tournament.Description,
                GameId = tournament.GameId,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                RegistrationDeadline = tournament.RegistrationDeadline,
                MaxTeams = tournament.MaxTeams,
                MinTeamSize = tournament.MinTeamSize,
                MaxTeamSize = tournament.MaxTeamSize,
                PrizePool = tournament.PrizePool,
                ImageUrl = tournament.ImageUrl,
                OrganizerId = tournament.OrganizerId,
                CreatedDate = tournament.CreatedDate,
                RegisteredTeamsCount = 0
            };

            return CreatedAtAction(nameof(GetTournament), new { id = tournament.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTournament(string id, [FromBody] UpdateTournamentDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var gameExists = await _context.Games.AnyAsync(g => g.Id == updateDto.GameId);
            if (!gameExists) return BadRequest("O jogo indicado não existe.");

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();

            tournament.Name = updateDto.Name;
            tournament.Description = updateDto.Description;
            tournament.GameId = updateDto.GameId;
            tournament.StartDate = updateDto.StartDate;
            tournament.EndDate = updateDto.EndDate;
            tournament.RegistrationDeadline = updateDto.RegistrationDeadline ?? tournament.RegistrationDeadline;
            tournament.MaxTeams = updateDto.MaxTeams;
            tournament.MinTeamSize = updateDto.MinTeamSize;
            tournament.MaxTeamSize = updateDto.MaxTeamSize;
            tournament.PrizePool = updateDto.PrizePool;
            if (!string.IsNullOrWhiteSpace(updateDto.ImageUrl))
            {
                tournament.ImageUrl = updateDto.ImageUrl;
            }

            _context.Tournaments.Update(tournament);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTournament(string id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();

            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}