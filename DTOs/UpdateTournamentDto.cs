using System;
using System.ComponentModel.DataAnnotations;

namespace Esportify.DTOs
{
    public class UpdateTournamentDto
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public string GameId { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public DateTime? RegistrationDeadline { get; set; }

        [Range(1, 1024)]
        public int MaxTeams { get; set; } = 16;

        [Range(1, 128)]
        public int MinTeamSize { get; set; } = 3;

        [Range(1, 128)]
        public int MaxTeamSize { get; set; } = 5;

        [Range(0, double.MaxValue)]
        public decimal PrizePool { get; set; } = 1000m;

        [Url]
        public string? ImageUrl { get; set; }
    }
}
