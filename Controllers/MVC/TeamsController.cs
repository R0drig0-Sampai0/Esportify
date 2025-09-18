using Esportify.Data;
using Esportify.Models;
using Esportify.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using System.Security.Claims;

[Authorize]
public class TeamsController : Controller
{
    private readonly EsportifyContext _context;

    public TeamsController(EsportifyContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(int page = 1)
    {
        int pageSize = 12;

        var teams = await _context.Teams
            .Include(t => t.Members)
            .ThenInclude(tm => tm.User) 
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalTeams = await _context.Teams.CountAsync();
        ViewBag.TotalPages = (int)Math.Ceiling(totalTeams / (double)pageSize);
        ViewBag.CurrentPage = page;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        List<string> userTeamIds = new List<string>();
        if (userId != null)
        {
            userTeamIds = await _context.TeamMembers
                                        .Where(tm => tm.UserId == userId)
                                        .Select(tm => tm.TeamId)
                                        .ToListAsync();
        }
        ViewBag.UserTeamIds = userTeamIds;

        return View(teams);
    }

    // GET: Create Team Form
    public IActionResult Create()
    {
        return View();
    }

    // POST: Create Team
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TeamViewModel model, IFormFile logoFile)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        string logoUrl = null;
        
        // Handle logo upload
        if (logoFile != null && logoFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(logoFile.FileName);
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "teams");
            
            // Create directory if it doesn't exist
            Directory.CreateDirectory(uploadsPath);
            
            var filePath = Path.Combine(uploadsPath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await logoFile.CopyToAsync(stream);
            }
            
            logoUrl = $"/images/teams/{fileName}";
        }

        var team = new Team
        {
            Id = Guid.NewGuid().ToString(),
            Name = model.Name,
            Description = model.Description ?? string.Empty,
            Tag = model.Tag,
            LogoUrl = logoUrl,
            IsOpenForMembers = model.IsOpenForMembers,
            CreatedAt = DateTime.UtcNow,
            LeaderId = userId,
            Members = new List<TeamMember>()
        };

        try
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            // Add the leader as a member too
            _context.TeamMembers.Add(new TeamMember
            {
                TeamId = team.Id,
                UserId = userId,
                Role = "Leader",
                JoinedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Equipa criada com sucesso!";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Erro ao criar equipa: " + ex.Message;
            return View(model);
        }
    }

    // GET: Teams/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var team = await _context.Teams
            .Include(t => t.Leader)
            .Include(t => t.Members)
                .ThenInclude(tm => tm.User)
                    .ThenInclude(u => u.Profile)
            .Include(t => t.Registrations)
                .ThenInclude(r => r.Tournament)
                    .ThenInclude(t => t.Game)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        return View(team);
    }

    // GET: Teams/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var team = await _context.Teams
            .Include(t => t.Members)
                .ThenInclude(tm => tm.User)
                    .ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        // Only team leader or admin can edit
        if (team.LeaderId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(team);
    }

    // POST: Teams/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Team team, IFormFile logoFile)
    {
        if (id != team.Id)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var existingTeam = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (existingTeam == null)
        {
            return NotFound();
        }

        // Only team leader or admin can edit
        if (existingTeam.LeaderId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Update basic properties
                existingTeam.Name = team.Name;
                existingTeam.Description = team.Description ?? string.Empty;
                existingTeam.Tag = team.Tag;
                existingTeam.IsOpenForMembers = team.IsOpenForMembers;

                // Handle logo upload
                if (logoFile != null && logoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(logoFile.FileName);
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "teams");

                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(uploadsPath);

                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await logoFile.CopyToAsync(stream);
                    }

                    existingTeam.LogoUrl = $"/images/teams/{fileName}";
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Equipa atualizada com sucesso!";
                return RedirectToAction("Details", new { id = team.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erro ao atualizar equipa: " + ex.Message;
            }
        }

        // Reload team data for the view
        existingTeam = await _context.Teams
            .Include(t => t.Members)
                .ThenInclude(tm => tm.User)
                    .ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(t => t.Id == id);

        return View(existingTeam);
    }

    // POST: Teams/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var team = await _context.Teams
            .Include(t => t.Members)
            .Include(t => t.Registrations)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (team == null)
        {
            return NotFound();
        }

        // Only team leader or admin can delete
        if (team.LeaderId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        try
        {
            // Remove all team members first
            _context.TeamMembers.RemoveRange(team.Members);

            // Remove all registrations
            _context.Registrations.RemoveRange(team.Registrations);

            // Remove the team
            _context.Teams.Remove(team);

            await _context.SaveChangesAsync();
            TempData["Success"] = "Equipa eliminada com sucesso!";
        }
        catch (Exception ex)
        {
        }

        return RedirectToAction("Index");
    }

    // POST: Teams/Join/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> JoinTeam(string teamId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            TempData["Error"] = "Equipa não encontrada.";
            return RedirectToAction("Index");
        }

        // Check if team is open for new members
        if (!team.IsOpenForMembers)
        {
            TempData["Error"] = "Esta equipa não está aberta para novos membros.";
            return RedirectToAction("Details", new { id = teamId });
        }

        // Check if user is already a member
        var existingMembership = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

        if (existingMembership != null)
        {
            TempData["Error"] = "Já és membro desta equipa.";
            return RedirectToAction("Details", new { id = teamId });
        }

        try
        {
            // Add user to team
            var teamMember = new TeamMember
            {
                TeamId = teamId,
                UserId = userId,
                Role = "Member",
                JoinedAt = DateTime.UtcNow
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Juntaste-te à equipa {team.Name} com sucesso!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Erro ao juntar-se à equipa: " + ex.Message;
        }

        return RedirectToAction("Details", new { id = teamId });
    }

    // POST: Teams/Leave/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveTeam(string teamId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var team = await _context.Teams
            .Include(t => t.Members)
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
        {
            TempData["Error"] = "Equipa não encontrada.";
            return RedirectToAction("Index");
        }

        // Check if user is the team leader
        if (team.LeaderId == userId)
        {
            TempData["Error"] = "Não podes sair da equipa sendo o líder. Transfere a liderança primeiro.";
            return RedirectToAction("Details", new { id = teamId });
        }

        // Check if user is actually a member
        var membership = await _context.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

        if (membership == null)
        {
            TempData["Error"] = "Não és membro desta equipa.";
            return RedirectToAction("Details", new { id = teamId });
        }

        try
        {
            _context.TeamMembers.Remove(membership);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Saíste da equipa {team.Name} com sucesso.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Erro ao sair da equipa: " + ex.Message;
        }

        return RedirectToAction("Index");
    }
}
