using Esportify.Data;
using Esportify.DTOs;
using Esportify.Hubs;
using Esportify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<TournamentHub> _tournamentHubContext;

        public TournamentsApiController(
            EsportifyContext context,
            IHubContext<TournamentHub> tournamentHubContext)
        {
            _context = context;
            _tournamentHubContext = tournamentHubContext;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTournaments(string? search, string? gameId, bool onlyOpen = false)
        {
            var query = _context.Tournaments.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Name.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(gameId))
            {
                query = query.Where(t => t.GameId == gameId);
            }

            if (onlyOpen)
            {
                var now = DateTime.UtcNow;
                query = query.Where(t =>
                    t.RegistrationDeadline >= now &&
                    t.Registrations.Count() < t.MaxTeams);
            }

            var tournaments = await query
                .Select(t => new TournamentDto
                {
                    Id = t.Id ?? string.Empty,
                    Name = t.Name ?? string.Empty,
                    Description = t.Description ?? string.Empty,
                    GameId = t.GameId ?? string.Empty,
                    Game = t.Game != null
                        ? new GameDto
                        {
                            Id = t.Game.Id ?? string.Empty,
                            Name = t.Game.Name ?? string.Empty,
                            Genre = t.Game.Genre,
                            ImageUrl = t.Game.ImageUrl ?? string.Empty,
                            OfficialWebsite = t.Game.OfficialWebsite,
                            CreatedAt = t.Game.CreatedAt
                        }
                        : null,
                    StartDate = (DateTime?)t.StartDate ?? default,
                    EndDate = (DateTime?)t.EndDate ?? default,
                    RegistrationDeadline = (DateTime?)t.RegistrationDeadline,
                    MaxTeams = (int?)t.MaxTeams ?? 0,
                    MinTeamSize = (int?)t.MinTeamSize ?? 0,
                    MaxTeamSize = (int?)t.MaxTeamSize ?? 0,
                    PrizePool = (decimal?)t.PrizePool ?? 0,
                    ImageUrl = t.ImageUrl ?? string.Empty,
                    OrganizerId = t.OrganizerId,
                    OrganizerName = t.Organizer != null ? (t.Organizer.Profile != null ? t.Organizer.Profile.DisplayName ?? t.Organizer.UserName : t.Organizer.UserName) : null,
                    CreatedDate = (DateTime?)t.CreatedDate ?? default,
                    RegisteredTeamsCount = t.Registrations.Count()
                })
                .ToListAsync();

            return Ok(tournaments);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTournament(string id)
        {
            var tournament = await _context.Tournaments
                .Where(t => t.Id == id)
                .Select(t => new TournamentDto
                {
                    Id = t.Id ?? string.Empty,
                    Name = t.Name ?? string.Empty,
                    Description = t.Description ?? string.Empty,
                    GameId = t.GameId ?? string.Empty,
                    Game = t.Game != null
                        ? new GameDto
                        {
                            Id = t.Game.Id ?? string.Empty,
                            Name = t.Game.Name ?? string.Empty,
                            Genre = t.Game.Genre,
                            ImageUrl = t.Game.ImageUrl ?? string.Empty,
                            OfficialWebsite = t.Game.OfficialWebsite,
                            CreatedAt = t.Game.CreatedAt
                        }
                        : null,
                    StartDate = (DateTime?)t.StartDate ?? default,
                    EndDate = (DateTime?)t.EndDate ?? default,
                    RegistrationDeadline = (DateTime?)t.RegistrationDeadline,
                    MaxTeams = (int?)t.MaxTeams ?? 0,
                    MinTeamSize = (int?)t.MinTeamSize ?? 0,
                    MaxTeamSize = (int?)t.MaxTeamSize ?? 0,
                    PrizePool = (decimal?)t.PrizePool ?? 0,
                    ImageUrl = t.ImageUrl ?? string.Empty,
                    OrganizerId = t.OrganizerId,
                    OrganizerName = t.Organizer != null ? (t.Organizer.Profile != null ? t.Organizer.Profile.DisplayName ?? t.Organizer.UserName : t.Organizer.UserName) : null,
                    CreatedDate = (DateTime?)t.CreatedDate ?? default,
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

            var validationError = ValidateTournamentRules(
                createDto.StartDate,
                createDto.EndDate,
                createDto.RegistrationDeadline ?? createDto.StartDate,
                createDto.MinTeamSize,
                createDto.MaxTeamSize,
                createDto.MaxTeams);

            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

            var gameExists = await _context.Games.AnyAsync(g => g.Id == createDto.GameId);
            if (!gameExists) return BadRequest(new { message = "O jogo indicado não existe." });

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
                Id = tournament.Id ?? string.Empty,
                Name = tournament.Name ?? string.Empty,
                Description = tournament.Description ?? string.Empty,
                GameId = tournament.GameId ?? string.Empty,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                RegistrationDeadline = tournament.RegistrationDeadline,
                MaxTeams = tournament.MaxTeams,
                MinTeamSize = tournament.MinTeamSize,
                MaxTeamSize = tournament.MaxTeamSize,
                PrizePool = tournament.PrizePool,
                ImageUrl = tournament.ImageUrl ?? string.Empty,
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
            if (!gameExists) return BadRequest(new { message = "O jogo indicado não existe." });

            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tournament.OrganizerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var validationError = ValidateTournamentRules(
                updateDto.StartDate,
                updateDto.EndDate,
                updateDto.RegistrationDeadline ?? tournament.RegistrationDeadline,
                updateDto.MinTeamSize,
                updateDto.MaxTeamSize,
                updateDto.MaxTeams);

            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (tournament.OrganizerId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            _context.Tournaments.Remove(tournament);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Endpoints para gerir inscrições de equipas em torneios

        [HttpGet("{tournamentId}/registrations")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRegistrations(string tournamentId)
        {
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament == null) return NotFound(new { message = "O torneio não existe." });

            var registrations = await _context.Registrations
                .Where(r => r.TournamentId == tournamentId)
                .Select(r => new RegistrationDto
                {
                    TeamId = r.TeamId ?? string.Empty,
                    TeamName = r.Team != null ? r.Team.Name ?? string.Empty : string.Empty,
                    TeamTag = r.Team != null ? r.Team.Tag : null,
                    RegistrationDate = r.RegistrationDate
                })
                .ToListAsync();

            return Ok(registrations);
        }

        [HttpPost("{tournamentId}/registrations")]
        public async Task<IActionResult> RegisterTeam(string tournamentId, [FromBody] CreateRegistrationDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validar se o torneio existe
            var tournament = await _context.Tournaments.FindAsync(tournamentId);
            if (tournament == null) return NotFound(new { message = "O torneio não existe." });

            // Validar se a equipa existe
            var team = await _context.Teams.FindAsync(createDto.TeamId);
            if (team == null) return BadRequest(new { message = "A equipa não existe." });

            // Validar se a equipa já está inscrita
            var alreadyRegistered = await _context.Registrations
                .AnyAsync(r => r.TournamentId == tournamentId && r.TeamId == createDto.TeamId);
            if (alreadyRegistered) return BadRequest(new { message = "A equipa já está inscrita neste torneio." });

            // Validar se o torneio ainda aceita inscrições (RegistrationDeadline)
            if (DateTime.UtcNow > tournament.RegistrationDeadline)
                return BadRequest(new { message = "O prazo de inscrição para este torneio expirou." });

            // Validar se o número máximo de equipas não foi atingido
            var registeredCount = await _context.Registrations
                .CountAsync(r => r.TournamentId == tournamentId);
            if (registeredCount >= tournament.MaxTeams)
                return BadRequest(new { message = "Este torneio já atingiu o número máximo de equipas inscritas." });

            // Validar se o tamanho da equipa cumpre as regras do torneio
            var teamMembersCount = await _context.TeamMembers
                .CountAsync(tm => tm.TeamId == createDto.TeamId);

            if (teamMembersCount < tournament.MinTeamSize)
            {
                return BadRequest(new { message = $"A equipa deve ter pelo menos {tournament.MinTeamSize} membros." });
            }

            if (teamMembersCount > tournament.MaxTeamSize)
            {
                return BadRequest(new { message = $"A equipa deve ter no máximo {tournament.MaxTeamSize} membros." });
            }

            // Criar a inscrição
            var registration = new Registration
            {
                TournamentId = tournamentId,
                TeamId = createDto.TeamId,
                RegistrationDate = DateTime.UtcNow
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            await _tournamentHubContext.Clients
                .Group($"tournament-{tournamentId}")
                .SendAsync("TeamRegistered", new
                {
                    tournamentId,
                    teamId = registration.TeamId ?? string.Empty,
                    teamName = team.Name ?? string.Empty,
                    teamTag = team.Tag,
                    registrationDate = registration.RegistrationDate
                });

            var dto = new RegistrationDto
            {
                TeamId = registration.TeamId ?? string.Empty,
                TeamName = team.Name ?? string.Empty,
                TeamTag = team.Tag,
                RegistrationDate = registration.RegistrationDate
            };

            return Ok(new { message = "Equipa inscrita com sucesso.", data = dto });
        }

        [HttpDelete("{tournamentId}/registrations/{teamId}")]
        public async Task<IActionResult> UnregisterTeam(string tournamentId, string teamId)
        {
            // Validar se a inscrição existe
            var registration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.TournamentId == tournamentId && r.TeamId == teamId);
            if (registration == null) return NotFound(new { message = "A inscrição não existe." });

            var team = await _context.Teams
                .Where(t => t.Id == teamId)
                .Select(t => new
                {
                    TeamName = t.Name ?? string.Empty,
                    TeamTag = t.Tag
                })
                .FirstOrDefaultAsync();

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            await _tournamentHubContext.Clients
                .Group($"tournament-{tournamentId}")
                .SendAsync("TeamUnregistered", new
                {
                    tournamentId,
                    teamId,
                    teamName = team?.TeamName ?? string.Empty,
                    teamTag = team?.TeamTag
                });

            return NoContent();
        }

        private static string? ValidateTournamentRules(
            DateTime startDate,
            DateTime endDate,
            DateTime registrationDeadline,
            int minTeamSize,
            int maxTeamSize,
            int maxTeams)
        {
            if (startDate >= endDate)
            {
                return "A data de início deve ser anterior à data de fim.";
            }

            if (registrationDeadline > startDate)
            {
                return "O prazo de inscrição deve ser anterior ou igual à data de início.";
            }

            if (minTeamSize <= 0)
            {
                return "O tamanho mínimo da equipa deve ser maior que 0.";
            }

            if (maxTeamSize < minTeamSize)
            {
                return "O tamanho máximo da equipa deve ser maior ou igual ao tamanho mínimo.";
            }

            if (maxTeams <= 0)
            {
                return "O número máximo de equipas deve ser maior que 0.";
            }

            return null;
        }
    }
}
