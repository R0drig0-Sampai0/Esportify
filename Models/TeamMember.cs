namespace Esportify.Models
{
    public class TeamMember
    {
        /// <summary>
        /// Identificador único do membro da equipa.
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// Equipa à qual o membro pertence.
        /// </summary>
        public Team Team { get; set; } = null!;

        /// <summary>
        /// Identificador único do utilizador que é membro da equipa.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Utilizador que é membro da equipa.
        /// </summary>
        public User User { get; set; } = null!;

        /// <summary>
        /// Data e hora em que o membro se juntou à equipa.
        /// </summary>
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
