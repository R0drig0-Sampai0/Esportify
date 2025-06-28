using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Esportify.Models
{
    public class UserProfile
    {
        public User User { get; set; } = null!;

        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; } 

        [StringLength(256)]
        public string Bio { get; set; } = string.Empty;

        /// <summary>
        /// Imagem de perfil do utilizador.
        /// </summary>
        public string AvatarUrl { get; set; } = string.Empty;

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

        // Game stats
        public int TotalMatchesPlayed { get; set; } = 0;
        public int TournamentsWon { get; set; } = 0;
        public int TournamentsJoined { get; set; } = 0;

        // Preferences
        public string? FavoriteGame { get; set; }
        public string? FavoriteTeam { get; set; }

    }
}