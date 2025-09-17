using Esportify.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Esportify.Data.Initializers
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(EsportifyContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed Users if none exist
            if (!context.Users.Any())
            {
                var users = new[]
                {
                    new User
                    {
                        Id = "10000000-0000-0000-0000-000000000001",
                        UserName = "admin",
                        Email = "admin@esportify.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        IsAdmin = true
                    },
                    new User
                    {
                        Id = "10000000-0000-0000-0000-000000000002",
                        UserName = "player1",
                        Email = "player1@esportify.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Player1@123"),
                        IsAdmin = false
                    },
                    new User
                    {
                        Id = "10000000-0000-0000-0000-000000000003",
                        UserName = "player2",
                        Email = "player2@esportify.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Player2@123"),
                        IsAdmin = false
                    }
                };

                context.Users.AddRange(users);
                await context.SaveChangesAsync();
            }

            // Seed UserProfiles if none exist
            if (!context.UserProfiles.Any())
            {
                var profiles = new[]
                {
                    new UserProfile
                    {
                        UserId = "10000000-0000-0000-0000-000000000001",
                        AvatarUrl = "https://e7.pngegg.com/pngimages/799/987/png-clipart-computer-icons-avatar-icon-design-avatar-heroes-computer-wallpaper-thumbnail.png",
                        Earnings = 0m,
                        DisplayName = "Admin",
                        Bio = "Administrador da plataforma Esportify",
                        TotalMatchesPlayed = 0,
                        TournamentsWon = 0,
                        TournamentsJoined = 0
                    },
                    new UserProfile
                    {
                        UserId = "10000000-0000-0000-0000-000000000002",
                        AvatarUrl = "https://e7.pngegg.com/pngimages/340/946/png-clipart-avatar-user-computer-icons-software-developer-avatar-child-face-thumbnail.png",
                        Earnings = 0m,
                        DisplayName = "Player One",
                        Bio = "Jogador de esports",
                        TotalMatchesPlayed = 0,
                        TournamentsWon = 0,
                        TournamentsJoined = 0
                    },
                    new UserProfile
                    {
                        UserId = "10000000-0000-0000-0000-000000000003",
                        AvatarUrl = "https://e7.pngegg.com/pngimages/340/946/png-clipart-avatar-user-computer-icons-software-developer-avatar-child-face-thumbnail.png",
                        Earnings = 0m,
                        DisplayName = "Player Two",
                        Bio = "Jogador de esports",
                        TotalMatchesPlayed = 0,
                        TournamentsWon = 0,
                        TournamentsJoined = 0
                    }
                };

                context.UserProfiles.AddRange(profiles);
                await context.SaveChangesAsync();
            }

            // Seed Games if none exist
            if (!context.Games.Any())
            {
                var games = new[]
                {
                    new Game { Id = "1a2b3c4d-0001", Name = "League of Legends", Genre = "MOBA", ImageUrl = "https://cdn2.steamgriddb.com/thumb/739ba8f780c1b9a7bc5e17d61cb8cb41.png", OfficialWebsite = "https://www.leagueoflegends.com" },
                    new Game { Id = "1a2b3c4d-0002", Name = "Valorant", Genre = "FPS", ImageUrl = "https://cdn2.steamgriddb.com/thumb/9edb6b9b7fc3b263b86740c635839dc4.jpg", OfficialWebsite = "https://playvalorant.com" },
                    new Game { Id = "1a2b3c4d-0003", Name = "Dota 2", Genre = "MOBA", ImageUrl = "https://cdn2.steamgriddb.com/thumb/cd94516dfa071f5f8b6b4eb4e24271e6.png", OfficialWebsite = "https://www.dota2.com" },
                    new Game { Id = "1a2b3c4d-0004", Name = "Counter-Strike 2", Genre = "FPS", ImageUrl = "https://cdn2.steamgriddb.com/thumb/0662aa1719017e0efa5fa8daf0880c6e.jpg", OfficialWebsite = "https://www.counter-strike.net" },
                    new Game { Id = "1a2b3c4d-0005", Name = "Overwatch 2", Genre = "FPS", ImageUrl = "https://cdn2.steamgriddb.com/thumb/1f866a4d80ffe25a0f62eaab16f7d59d.jpg", OfficialWebsite = "https://playoverwatch.com" },
                    new Game { Id = "1a2b3c4d-0006", Name = "Fortnite", Genre = "Battle Royale", ImageUrl = "https://cdn2.steamgriddb.com/thumb/6c4d541fc68d426aa028bc05f38164d1.jpg", OfficialWebsite = "https://www.epicgames.com/fortnite" },
                    new Game { Id = "1a2b3c4d-0007", Name = "Apex Legends", Genre = "Battle Royale", ImageUrl = "https://cdn2.steamgriddb.com/thumb/0d7a3aef18b1eb97e70a5148e2a2646f.jpg", OfficialWebsite = "https://www.ea.com/games/apex-legends" },
                    new Game { Id = "1a2b3c4d-0008", Name = "Rocket League", Genre = "Sports", ImageUrl = "https://cdn2.steamgriddb.com/thumb/0e765b51996b81c6ccf9a63a943a2dc7.jpg", OfficialWebsite = "https://www.rocketleague.com" },
                    new Game { Id = "1a2b3c4d-0009", Name = "Call of Duty: Warzone", Genre = "Battle Royale", ImageUrl = "https://cdn2.steamgriddb.com/thumb/da64a736b47e9b601309a3a86f013db0.jpg", OfficialWebsite = "https://www.callofduty.com/warzone" },
                    new Game { Id = "1a2b3c4d-0010", Name = "PUBG", Genre = "Battle Royale", ImageUrl = "https://cdn2.steamgriddb.com/thumb/13816ba0dd3a36209cbc3cfef265dc7c.jpg", OfficialWebsite = "https://pubg.com" },
                    new Game { Id = "1a2b3c4d-0011", Name = "Hearthstone", Genre = "Card Game", ImageUrl = "https://cdn2.steamgriddb.com/thumb/d9ff4e5efa087c26ac440ffcf4ad83f9.jpg", OfficialWebsite = "https://playhearthstone.com" },
                    new Game { Id = "1a2b3c4d-0012", Name = "FIFA 23", Genre = "Sports", ImageUrl = "https://cdn2.steamgriddb.com/thumb/19c054c2532d0e05cbd4451ae32b43cd.jpg", OfficialWebsite = "https://www.ea.com/games/fifa" },
                    new Game { Id = "1a2b3c4d-0013", Name = "Street Fighter 6", Genre = "Fighting", ImageUrl = "https://cdn2.steamgriddb.com/thumb/29d0d6e6e44c1ab6933ac3a3a0fc147f.jpg", OfficialWebsite = "https://www.streetfighter.com" },
                    new Game { Id = "1a2b3c4d-0014", Name = "Tekken 8", Genre = "Fighting", ImageUrl = "https://cdn2.steamgriddb.com/thumb/698c42c967a68312997479c5bde6cb28.jpg", OfficialWebsite = "https://www.tekken.com" },
                    new Game { Id = "1a2b3c4d-0015", Name = "Minecraft", Genre = "Sandbox", ImageUrl = "https://cdn2.steamgriddb.com/thumb/a73027901f88055aaa0fd1a9e25d36c7.jpg", OfficialWebsite = "https://www.minecraft.net" },
                    new Game { Id = "1a2b3c4d-0016", Name = "Super Smash Bros. Ultimate", Genre = "Fighting", ImageUrl = "https://cdn2.steamgriddb.com/thumb/2ef85f2ae5e56041ded26f67e18136be.jpg", OfficialWebsite = "https://www.smashbros.com" },
                    new Game { Id = "1a2b3c4d-0017", Name = "Team Fortress 2", Genre = "FPS", ImageUrl = "https://cdn2.steamgriddb.com/thumb/139b23924c6bb8418a337b563518d83a.jpg", OfficialWebsite = "https://www.teamfortress.com" },
                    new Game { Id = "1a2b3c4d-0018", Name = "Rainbow Six Siege", Genre = "FPS", ImageUrl = "https://cdn2.steamgriddb.com/thumb/c32d0e02132a853fdf5b8010aa71602e.jpg", OfficialWebsite = "https://www.ubisoft.com/game/rainbow-six/siege" },
                    new Game { Id = "1a2b3c4d-0019", Name = "StarCraft II", Genre = "RTS", ImageUrl = "https://cdn2.steamgriddb.com/thumb/9876b43ac1d19e78b15dade574ebfd9a.jpg", OfficialWebsite = "https://starcraft2.com" },
                    new Game { Id = "1a2b3c4d-0020", Name = "World of Warcraft", Genre = "MMORPG", ImageUrl = "https://cdn2.steamgriddb.com/thumb/9241c94d36aad903902fcbc473282aa8.jpg", OfficialWebsite = "https://worldofwarcraft.com" }
                };

                context.Games.AddRange(games);
                await context.SaveChangesAsync();
            }

            // Seed Tournaments if none exist
            if (!context.Tournaments.Any())
            {
                var random = new Random();
                var games = await context.Games.ToListAsync();
                var adminId = "10000000-0000-0000-0000-000000000001";

                var tournaments = new[]
                {
        // MOBA Tournaments
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "League of Legends World Championship",
            Description = "The biggest LoL tournament of the year",
            GameId = games.First(g => g.Name == "League of Legends").Id,
            StartDate = DateTime.Now.AddDays(30),
            EndDate = DateTime.Now.AddDays(37),
            RegistrationDeadline = DateTime.Now.AddDays(20),
            MaxTeams = 16,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 2000000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/5fa68a32c8ce2feebc132fa808d1ec0a.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Dota 2 The International",
            Description = "Annual Dota 2 world championship tournament",
            GameId = games.First(g => g.Name == "Dota 2").Id,
            StartDate = DateTime.Now.AddDays(45),
            EndDate = DateTime.Now.AddDays(52),
            RegistrationDeadline = DateTime.Now.AddDays(35),
            MaxTeams = 18,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 3000000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/ea7c72fb7e9e2b6c0fdbc61050023187.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },

        // FPS Tournaments
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Valorant Champions Tour",
            Description = "Global Valorant championship series",
            GameId = games.First(g => g.Name == "Valorant").Id,
            StartDate = DateTime.Now.AddDays(15),
            EndDate = DateTime.Now.AddDays(17),
            RegistrationDeadline = DateTime.Now.AddDays(10),
            MaxTeams = 32,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 1000000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/b46469c5851be7facec604ceeef9aaaf.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "CS:2 Major Championship",
            Description = "Premier CS:2 tournament",
            GameId = games.First(g => g.Name == "Counter-Strike 2").Id,
            StartDate = DateTime.Now.AddDays(60),
            EndDate = DateTime.Now.AddDays(65),
            RegistrationDeadline = DateTime.Now.AddDays(50),
            MaxTeams = 24,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 1500000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/8baf0b49e5213e010109d31bdf591fd6.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Overwatch 2 World Cup",
            Description = "National teams compete in Overwatch 2",
            GameId = games.First(g => g.Name == "Overwatch 2").Id,
            StartDate = DateTime.Now.AddDays(25),
            EndDate = DateTime.Now.AddDays(28),
            RegistrationDeadline = DateTime.Now.AddDays(15),
            MaxTeams = 20,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 750000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/20e29904d611f1e9dd8728bcae233854.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },

        // Battle Royale Tournaments
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Fortnite World Cup",
            Description = "Solo and duo Fortnite competition",
            GameId = games.First(g => g.Name == "Fortnite").Id,
            StartDate = DateTime.Now.AddDays(40),
            EndDate = DateTime.Now.AddDays(42),
            RegistrationDeadline = DateTime.Now.AddDays(30),
            MaxTeams = 100,
            MinTeamSize = 1,
            MaxTeamSize = 2,
            PrizePool = 3000000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/6b41f181551b67d57d104272001e13cb.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Apex Legends Global Series",
            Description = "Premier Apex Legends tournament",
            GameId = games.First(g => g.Name == "Apex Legends").Id,
            StartDate = DateTime.Now.AddDays(20),
            EndDate = DateTime.Now.AddDays(22),
            RegistrationDeadline = DateTime.Now.AddDays(10),
            MaxTeams = 60,
            MinTeamSize = 3,
            MaxTeamSize = 3,
            PrizePool = 1000000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/f40b4ef340a1e7f56c744ed2287c77c1.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },

        // Sports Tournaments
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Rocket League Championship",
            Description = "Top Rocket League teams compete",
            GameId = games.First(g => g.Name == "Rocket League").Id,
            StartDate = DateTime.Now.AddDays(35),
            EndDate = DateTime.Now.AddDays(37),
            RegistrationDeadline = DateTime.Now.AddDays(25),
            MaxTeams = 16,
            MinTeamSize = 3,
            MaxTeamSize = 3,
            PrizePool = 500000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/400af71a935022ba0b7d8cf49e412984.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "FIFA 23 World Cup",
            Description = "1v1 FIFA tournament",
            GameId = games.First(g => g.Name == "FIFA 23").Id,
            StartDate = DateTime.Now.AddDays(50),
            EndDate = DateTime.Now.AddDays(52),
            RegistrationDeadline = DateTime.Now.AddDays(40),
            MaxTeams = 64,
            MinTeamSize = 1,
            MaxTeamSize = 1,
            PrizePool = 250000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/51aa0970c09f34740671a6c41f92df51.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },

        // Fighting Tournaments
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Street Fighter 6 Masters",
            Description = "Premier Street Fighter competition",
            GameId = games.First(g => g.Name == "Street Fighter 6").Id,
            StartDate = DateTime.Now.AddDays(18),
            EndDate = DateTime.Now.AddDays(19),
            RegistrationDeadline = DateTime.Now.AddDays(10),
            MaxTeams = 32,
            MinTeamSize = 1,
            MaxTeamSize = 1,
            PrizePool = 200000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/245710681d51a6dfb80ab06683f3be01.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Tekken 8 World Tour",
            Description = "Global Tekken tournament series",
            GameId = games.First(g => g.Name == "Tekken 8").Id,
            StartDate = DateTime.Now.AddDays(42),
            EndDate = DateTime.Now.AddDays(43),
            RegistrationDeadline = DateTime.Now.AddDays(35),
            MaxTeams = 32,
            MinTeamSize = 1,
            MaxTeamSize = 1,
            PrizePool = 150000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/7db48ed21ea58bdbe6cc5f0865acccd8.png",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },

        // Other Tournaments
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Hearthstone Grandmasters",
            Description = "Top Hearthstone players compete",
            GameId = games.First(g => g.Name == "Hearthstone").Id,
            StartDate = DateTime.Now.AddDays(28),
            EndDate = DateTime.Now.AddDays(30),
            RegistrationDeadline = DateTime.Now.AddDays(20),
            MaxTeams = 16,
            MinTeamSize = 1,
            MaxTeamSize = 1,
            PrizePool = 300000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/7e0bb07735b79680eb1ee707248353fd.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Smash Bros Ultimate Summit",
            Description = "Invitational Smash Bros tournament",
            GameId = games.First(g => g.Name == "Super Smash Bros. Ultimate").Id,
            StartDate = DateTime.Now.AddDays(55),
            EndDate = DateTime.Now.AddDays(57),
            RegistrationDeadline = DateTime.Now.AddDays(45),
            MaxTeams = 16,
            MinTeamSize = 1,
            MaxTeamSize = 1,
            PrizePool = 100000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/5749ac926d0b4565b0594576f56858fd.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },

        // Regional Tournaments
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "North American LoL Open",
            Description = "Regional League of Legends tournament",
            GameId = games.First(g => g.Name == "League of Legends").Id,
            StartDate = DateTime.Now.AddDays(10),
            EndDate = DateTime.Now.AddDays(12),
            RegistrationDeadline = DateTime.Now.AddDays(5),
            MaxTeams = 16,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 50000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/67559abce761a57a4e0782a9f60e4adc.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "European Valorant Series",
            Description = "EU Valorant competition",
            GameId = games.First(g => g.Name == "Valorant").Id,
            StartDate = DateTime.Now.AddDays(22),
            EndDate = DateTime.Now.AddDays(24),
            RegistrationDeadline = DateTime.Now.AddDays(15),
            MaxTeams = 16,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 75000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/e0849f52064965cc7f5b564618f15663.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },

        // Amateur Tournaments
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "CS:2 Open Cup",
            Description = "Amateur CS:2 tournament",
            GameId = games.First(g => g.Name == "Counter-Strike 2").Id,
            StartDate = DateTime.Now.AddDays(8),
            EndDate = DateTime.Now.AddDays(9),
            RegistrationDeadline = DateTime.Now.AddDays(3),
            MaxTeams = 32,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 10000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/74d2bd1261cd7590e2acdfa9990b64e5.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Rocket League Community Cup",
            Description = "Open Rocket League tournament",
            GameId = games.First(g => g.Name == "Rocket League").Id,
            StartDate = DateTime.Now.AddDays(14),
            EndDate = DateTime.Now.AddDays(15),
            RegistrationDeadline = DateTime.Now.AddDays(7),
            MaxTeams = 32,
            MinTeamSize = 3,
            MaxTeamSize = 3,
            PrizePool = 5000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/90faf607cc2f5c89eb2107a72b06a2b7.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },

        // Special Event Tournaments
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Summer Showdown",
            Description = "Multi-game summer tournament",
            GameId = games.First(g => g.Name == "Valorant").Id,
            StartDate = DateTime.Now.AddDays(65),
            EndDate = DateTime.Now.AddDays(70),
            RegistrationDeadline = DateTime.Now.AddDays(55),
            MaxTeams = 32,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 250000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/276e4b3032ceaa7e52e9c0a41483d58f.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Winter Invitational",
            Description = "Holiday season tournament",
            GameId = games.First(g => g.Name == "League of Legends").Id,
            StartDate = DateTime.Now.AddDays(90),
            EndDate = DateTime.Now.AddDays(92),
            RegistrationDeadline = DateTime.Now.AddDays(80),
            MaxTeams = 16,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 100000,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/23a37e8274e55459d40e40e9798efbf7.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },

        // Charity Tournaments
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Gaming for Good",
            Description = "Charity tournament supporting children's hospitals",
            GameId = games.First(g => g.Name == "Fortnite").Id,
            StartDate = DateTime.Now.AddDays(38),
            EndDate = DateTime.Now.AddDays(39),
            RegistrationDeadline = DateTime.Now.AddDays(30),
            MaxTeams = 100,
            MinTeamSize = 1,
            MaxTeamSize = 4,
            PrizePool = 0,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/6dc3399d124d4cf965d4d62d0663228e.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        },
        new Tournament
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Esports Against Cancer",
            Description = "Charity tournament for cancer research",
            GameId = games.First(g => g.Name == "Dota 2").Id,
            StartDate = DateTime.Now.AddDays(75),
            EndDate = DateTime.Now.AddDays(77),
            RegistrationDeadline = DateTime.Now.AddDays(65),
            MaxTeams = 16,
            MinTeamSize = 5,
            MaxTeamSize = 5,
            PrizePool = 0,
            ImageUrl = "https://cdn2.steamgriddb.com/thumb/d8732349cbe3ba46021a86345bb98c4c.jpg",
            OrganizerId = adminId,
            CreatedDate = DateTime.Now
        }
    };

                context.Tournaments.AddRange(tournaments);
                await context.SaveChangesAsync();
            }
        }
    }
}