using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Esportify.Models;
using System.Security.Claims;
using Esportify.Data;

public class TeamsController : Controller
{
    private readonly EsportifyContext _context;

    public TeamsController(EsportifyContext context)
    {
        _context = context;
    }

    // GET: Teams
    public async Task<IActionResult> Index()
    {
        var teams = await _context.Teams
            .Include(t => t.Leader)
            .ToListAsync();
        return View(teams);
    }

    // GET: Teams/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Teams/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TeamName,Tag,IsOpenForMembers")] Team team, IFormFile logoFile)
    {
        if (ModelState.IsValid)
        {
            team.Id = Guid.NewGuid().ToString();
            team.CreatedAt = DateTime.UtcNow;
            team.LeaderId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Handle logo upload
            if (logoFile != null && logoFile.Length > 0)
            {
                var fileName = Path.GetFileName(logoFile.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await logoFile.CopyToAsync(stream);
                }
                team.LogoUrl = $"/uploads/{fileName}";
            }

            _context.Add(team);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(team);
    }
}