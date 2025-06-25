using System.ComponentModel.DataAnnotations;

namespace Esportify.Models
{
    public class Tournament
    {
        /// <summary>
        /// Identificador único do torneio.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do torneio.
        /// </summary>
        [Required(ErrorMessage = "O nome do torneio é de preenchimento obrigatório")]
        [StringLength(128)]
        public string TournamentName { get; set; } = string.Empty;

        /// <summary>
        /// Descrição do torneio.
        /// </summary>
        [StringLength(512)]
        public string? Description { get; set; }

        /// <summary>
        /// Data de início e fim do torneio.
        /// </summary>
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Identificador do jogo associado ao torneio.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Jogo associado ao torneio.
        /// </summary>
        public Game Game { get; set; }

        /// <summary>
        /// Identificador do criador do torneio (utilizador).
        /// </summary>
        public int CreatorId { get; set; }

        /// <summary>
        /// Criador do torneio, que é um utilizador.
        /// </summary>
        public User Creator { get; set; }

        /// <summary>
        /// Número máximo de equipas que podem participar no torneio.
        /// </summary>
        public int MaxTeams { get; set; }

        /// <summary>
        /// Tamanho da equipa, ou seja, o número de membros por equipa.
        /// </summary>
        public int TeamSize { get; set; }

        /// <summary>
        /// Status do torneio, como "Upcoming", "Ongoing", "Completed"
        /// </summary>
        public string Status { get; set; } = "Upcoming";

        /// <summary>
        /// Localização do torneio, por exemplo, "Online" ou "Presencial".
        /// </summary>
        public string Location { get; set; } = "Online";

        /// <summary>
        /// Prémio total do torneio.
        /// </summary>
        public string? PrizePool { get; set; }

        /****************************
        Definção dos relacionamentos
        ****************************/

        /// <summary>
        /// Lista de registos de torneios associados a este torneio.
        /// </summary>
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

    }
}
