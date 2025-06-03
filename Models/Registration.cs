namespace Esportify.Models
{
    public class Registration
    {
        /// <summary>
        /// Identificador único do registo de torneio.
        /// </summary>
        public int RegistrationId { get; set; }

        /// <summary>
        /// Identificador da equipa que se está a registar no torneio.
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// Equipa que está a registar-se no torneio.
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// Identificador do torneio no qual a equipa está a registar-se.
        /// </summary>
        public int TournamentId { get; set; }

        /// <summary>
        /// Torneio no qual a equipa está a registar-se.
        /// </summary>
        public Tournament Tournament { get; set; }

        /// <summary>
        /// Data e hora em que a equipa se registou no torneio.
        /// </summary>
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    }
}
