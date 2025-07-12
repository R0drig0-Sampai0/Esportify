using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Esportify.Models
{
    public class UserProfile
    {
        // Use UserId as the primary key to match User.Id (1-1 relationship)
        [Key]
        [ForeignKey("User")]
        public string UserId { get; set; }

        // Navigation property (required)
        public User User { get; set; } = null!;

        // Allow null to match seeding default
        [StringLength(256)]
        public string? DisplayName { get; set; }

        [StringLength(256)]
        public string? Bio { get; set; }

        /// <summary>
        /// Imagem de perfil do utilizador.
        /// </summary>
        [StringLength(256)]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Imagem de banner do utilizador.
        /// </summary>
        [StringLength(256)]
        public string? BannerUrl { get; set; }

        /// <summary>
        /// País do utilizador.
        /// </summary>
        [StringLength(64)]
        public string? Country { get; set; }

        [Url]
        public string? TwitchUrl { get; set; }

        [Url]
        public string? YouTubeUrl { get; set; }

        [Url]
        public string? TwitterUrl { get; set; }

        [Url]
        public string? DiscordUrl { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Earnings { get; set; }

        // Game stats
        public int TotalMatchesPlayed { get; set; }
        public int TournamentsWon { get; set; }
        public int TournamentsJoined { get; set; }

        // Preferences
        [StringLength(100)]
        public string? FavoriteGame { get; set; }

        [StringLength(100)]
        public string? FavoriteTeam { get; set; }
    }
}