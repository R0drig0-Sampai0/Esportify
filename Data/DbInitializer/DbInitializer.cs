using Esportify.Data;
using Esportify.Models;
using Microsoft.EntityFrameworkCore;

public static class DbInitializer
{
    public static void Initialize(EsportifyContext context)
    {
        Console.WriteLine("Starting database migration...");
        context.Database.Migrate();
        Console.WriteLine("Migration completed.");

        if (context.Users.Any())
        {
            Console.WriteLine("Database already contains users, skipping initialization.");
            return;
        }

        Console.WriteLine("Seeding users...");
        var users = new List<User>
        {
            new User { Username = "playerOne", Email = "player1@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password1"), IsEmailConfirmed = true },
            new User { Username = "gamerGirl", Email = "gamer@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password2"), IsEmailConfirmed = true },
            new User { Username = "adminX", Email = "admin@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password3"), IsEmailConfirmed = true }
        };
        context.Users.AddRange(users);
        context.SaveChanges();
        Console.WriteLine("Users seeded.");

        Console.WriteLine("Seeding games...");
        var games = new List<Game>
        {
            new Game { Name = "Valorant", Genre = "FPS", LogoUrl = "/images/valorant.png", OfficialWebsite = "https://playvalorant.com" },
            new Game { Name = "League of Legends", Genre = "MOBA", LogoUrl = "/images/lol.png", OfficialWebsite = "https://leagueoflegends.com" }
        };
        context.Games.AddRange(games);
        context.SaveChanges();
        Console.WriteLine("Games seeded.");

        Console.WriteLine("Seeding teams...");
        var teams = new List<Team>
        {
            new Team { TeamName = "Team Alpha", Tag = "ALPHA", LeaderId = users[0].Id, IsOpenForMembers = true },
            new Team { TeamName = "Omega Squad", Tag = "OMG", LeaderId = users[1].Id, IsOpenForMembers = false }
        };
        context.Teams.AddRange(teams);
        context.SaveChanges();
        Console.WriteLine("Teams seeded.");

        Console.WriteLine("Seeding team members...");
        var teamMembers = new List<TeamMember>
        {
            new TeamMember { TeamId = teams[0].Id, UserId = users[0].Id },
            new TeamMember { TeamId = teams[0].Id, UserId = users[2].Id },
            new TeamMember { TeamId = teams[1].Id, UserId = users[1].Id }
        };
        context.TeamMembers.AddRange(teamMembers);
        context.SaveChanges();
        Console.WriteLine("Team members seeded.");

        Console.WriteLine("Seeding tournaments...");
        var tournaments = new List<Tournament>
        {
            new Tournament
            {
                TournamentName = "Valorant Showdown",
                GameId = games[0].Id,
                CreatorId = users[2].Id,
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(12),
                MaxTeams = 8,
                TeamSize = 5,
                Location = "Online",
                PrizePool = "$1000"
            },
            new Tournament
            {
                TournamentName = "LoL Clash",
                GameId = games[1].Id,
                CreatorId = users[2].Id,
                StartDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(7),
                MaxTeams = 16,
                TeamSize = 5,
                Location = "Online",
                PrizePool = "$2000"
            }
        };
        context.Tournaments.AddRange(tournaments);
        context.SaveChanges();
        Console.WriteLine("Tournaments seeded.");

        Console.WriteLine("Seeding registrations...");
        var registrations = new List<Registration>
        {
            new Registration { TeamId = teams[0].Id, TournamentId = tournaments[0].Id },
            new Registration { TeamId = teams[1].Id, TournamentId = tournaments[1].Id }
        };
        context.Registrations.AddRange(registrations);
        context.SaveChanges();
        Console.WriteLine("Registrations seeded.");
    }
}