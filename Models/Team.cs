using System.ComponentModel.DataAnnotations;

namespace Esportify.Models
{
    public class Team
    {
        /// <summary>
        /// Identificador único da equipa.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nome da equipa.
        /// </summary>
        [StringLength(64)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Descrição sobre a equipa
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Sigla da equipa.
        /// </summary>
        public string? Tag { get; set; }

        /// <summary>
        /// URL do logo da equipa.
        /// </summary>
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Quando a equipa foi criada.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Indica se a equipa está aberta para novos membros.
        /// </summary>
        public bool IsOpenForMembers { get; set; } = false;

        /// <summary>
        /// Identificador do líder da equipa.
        /// </summary>
        public string LeaderId { get; set; }

        /// <summary>
        /// Líder da equipa, que é um utilizador.
        /// </summary>
        public User Leader { get; set; }

        /****************************
        Definção dos relacionamentos
        ****************************/

        /// <summary>
        /// Lista de registos de torneios associados a esta equipa.
        /// </summary>
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

        /// <summary>
        /// Lista de membros da equipa, que são utilizadores.
        /// </summary>
        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

    }

}
