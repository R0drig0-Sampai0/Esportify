using System.ComponentModel.DataAnnotations;

namespace Esportify.Models
{
    public class Game
    {
        /// <summary>
        /// Identificador único do jogo.
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// Nome do jogo.
        /// </summary>
        [Required(ErrorMessage = "O nome do jogo é obrigatório.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição do jogo.
        /// </summary>
        [StringLength(50)]
        public string? Genre { get; set; }

        /// <summary>
        /// URL do logo do jogo.
        /// </summary>
        public string ImageUrl { get; set; } = "/images/games/default.jpg";

        /// <summary>
        /// Site oficial do jogo.
        /// </summary>
        public string? OfficialWebsite { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /****************************
        Definção dos relacionamentos
        ****************************/

        /// <summary>
        /// Lista de torneios associados a este jogo.
        /// </summary>
        public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
        public ICollection<UserGame> LikedByUsers { get; set; } = new List<UserGame>();
    }

}
