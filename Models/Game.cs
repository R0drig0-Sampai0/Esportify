using System.ComponentModel.DataAnnotations;

namespace Esportify.Models
{
    public class Game
    {
        /// <summary>
        /// Identificador único do jogo.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do jogo.
        /// </summary>
        [Required(ErrorMessage = "O nome do jogo é de preenchimento obrigatório")]
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
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Site oficial do jogo.
        /// </summary>
        public string? OfficialWebsite { get; set; }

        /****************************
        Definção dos relacionamentos
        ****************************/

        /// <summary>
        /// Lista de torneios associados a este jogo.
        /// </summary>
        public ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();
    }

}
