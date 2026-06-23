using Esportify.Data;
using Esportify.DTOs;
using Esportify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Esportify.Controllers.API
{
    [Route("api/teams")]
    [ApiController]
    public class TeamsApiController : ControllerBase
    {
        private readonly EsportifyContext _context;

        public TeamsApiController(EsportifyContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTeams()
        {
            var teams = await _context.Teams
                .Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Tag = t.Tag,
                    LogoUrl = t.LogoUrl,
                    CreatedAt = t.CreatedAt,
                    IsOpenForMembers = t.IsOpenForMembers,
                    LeaderId = t.LeaderId,
                    LeaderUserName = t.Leader != null ? t.Leader.UserName : null,
                    MembersCount = t.Members.Count()
                })
                .ToListAsync();

            return Ok(teams);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTeam(string id)
        {
            var team = await _context.Teams
                .Where(t => t.Id == id)
                .Select(t => new TeamDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Tag = t.Tag,
                    LogoUrl = t.LogoUrl,
                    CreatedAt = t.CreatedAt,
                    IsOpenForMembers = t.IsOpenForMembers,
                    LeaderId = t.LeaderId,
                    LeaderUserName = t.Leader != null ? t.Leader.UserName : null,
                    MembersCount = t.Members.Count()
                })
                .FirstOrDefaultAsync();

            if (team == null) return NotFound();

            return Ok(team);
        }

        [HttpGet("{id}/members")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTeamMembers(string id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team == null) return NotFound();

            var members = await _context.TeamMembers
                .Where(tm => tm.TeamId == id)
                .Select(tm => new TeamMemberDto
                {
                    UserId = tm.UserId,
                    UserName = tm.User.UserName,
                    UserAvatarUrl = tm.User.Profile != null ? tm.User.Profile.AvatarUrl : null,
                    Role = tm.Role,
                    JoinedAt = tm.JoinedAt
                })
                .ToListAsync();

            return Ok(members);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTeam([FromBody] CreateTeamDto createDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var team = new Team
            {
                Id = Guid.NewGuid().ToString(),
                Name = createDto.Name,
                Description = createDto.Description ?? string.Empty,
                Tag = createDto.Tag,
                LogoUrl = createDto.LogoUrl,
                IsOpenForMembers = createDto.IsOpenForMembers,
                LeaderId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Teams.Add(team);
            _context.TeamMembers.Add(new TeamMember
            {
                TeamId = team.Id,
                UserId = userId,
                Role = "Leader",
                JoinedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var dto = new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                Description = team.Description,
                Tag = team.Tag,
                LogoUrl = team.LogoUrl,
                CreatedAt = team.CreatedAt,
                IsOpenForMembers = team.IsOpenForMembers,
                LeaderId = team.LeaderId,
                MembersCount = 1
            };

            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, dto);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateTeam(string id, [FromBody] UpdateTeamDto updateDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var team = await _context.Teams.FindAsync(id);
            if (team == null) return NotFound();

            if (team.LeaderId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            team.Name = updateDto.Name;
            team.Description = updateDto.Description ?? team.Description;
            team.Tag = updateDto.Tag;
            if (!string.IsNullOrWhiteSpace(updateDto.LogoUrl))
            {
                team.LogoUrl = updateDto.LogoUrl;
            }
            team.IsOpenForMembers = updateDto.IsOpenForMembers;

            _context.Teams.Update(team);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTeam(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var team = await _context.Teams
                .Include(t => t.Members)
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null) return NotFound();

            if (team.LeaderId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            try
            {
                _context.TeamMembers.RemoveRange(team.Members);
                _context.Registrations.RemoveRange(team.Registrations);
                _context.Teams.Remove(team);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return StatusCode(500, "Erro ao eliminar a equipa.");
            }

            return NoContent();
        }

        [HttpPost("{id}/join")]
        [Authorize]
        public async Task<IActionResult> JoinTeam(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var team = await _context.Teams
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null) return NotFound();

            if (!team.IsOpenForMembers)
            {
                return BadRequest("Esta equipa não está aberta para novos membros.");
            }

            var existingMembership = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == id && tm.UserId == userId);

            if (existingMembership != null)
            {
                return BadRequest("Já és membro desta equipa.");
            }

            var teamMember = new TeamMember
            {
                TeamId = id,
                UserId = userId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Juntaste-te à equipa com sucesso!" });
        }

        [HttpPost("{id}/leave")]
        [Authorize]
        public async Task<IActionResult> LeaveTeam(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == id && tm.UserId == userId);

            if (teamMember == null)
            {
                return BadRequest("Não és membro desta equipa.");
            }

            var team = await _context.Teams.FindAsync(id);
            if (team != null && team.LeaderId == userId)
            {
                return BadRequest("O líder da equipa não pode sair.");
            }

            _context.TeamMembers.Remove(teamMember);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Saíste da equipa com sucesso!" });
        }
    }
}
