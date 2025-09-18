using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Esportify.Models
{
    public class User : IdentityUser
    {

        public bool IsAdmin { get; set; }
        public bool IsOrganizer { get; set; }
        /****************************
         Definção dos relacionamentos
        ****************************/

        public UserProfile? Profile { get; set; }

        /// <summary>
        /// Lista de equipas que o utilizador faz parte.
        /// </summary>
        public ICollection<Team> Teams { get; set; } = new List<Team>();

        /// <summary>
        /// Lista de registos de torneios associados ao utilizador.
        /// </summary>
        public ICollection<Registration> Registrations { get; set; } = new List<Registration>();

        /// <summary>
        /// Lista de equipas que o utilizador pertence.
        /// </summary>
        public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();

        public ICollection<UserGame> FavoriteGames { get; set; } = new List<UserGame>();
    }
}
