using System;

namespace Esportify.DTOs
{
    public class TournamentDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string GameId { get; set; } = string.Empty;
        public GameDto? Game { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? RegistrationDeadline { get; set; }
        public int MaxTeams { get; set; }
        public int MinTeamSize { get; set; }
        public int MaxTeamSize { get; set; }
        public decimal PrizePool { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? OrganizerId { get; set; }
        public string? OrganizerName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int RegisteredTeamsCount { get; set; }
    }
}
