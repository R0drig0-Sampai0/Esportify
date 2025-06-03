using System.ComponentModel.DataAnnotations;

namespace Esportify.Models
{
    public class User
    {
        /// <summary>
        /// Identificador único do utilizador.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nome do utilizador.
        /// </summary>
        [Required(ErrorMessage = "O nome de utilizador é de preenchimento obrigatório.")]
        [StringLength(32, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de email do utilizador.
        /// </summary>
        [Required(ErrorMessage = "O email é de preenchimento obrigatório.")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Senha do utilizador, armazenada como um hash.
        /// </summary>
        [Required(ErrorMessage = "A password é de preenchimento obrigatório.")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Decrição do utilizador.
        /// </summary>
        [StringLength(256)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// País de morada do utilizador.
        /// </summary>
        [StringLength(64)]
        public string? Country { get; set; }

        /// <summary>
        /// URL da imagem de perfil do utilizador.
        /// </summary>
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Indica se o email do utilizador foi confirmado.
        /// </summary>
        public bool IsEmailConfirmed { get; set; } = false;

        /// <summary>
        /// Data e hora do último login do utilizador.
        /// </summary>
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Data e hora de criação do utilizador.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data e hora da última atualização do utilizador.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /****************************
         Definção dos relacionamentos
        ****************************/

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
    }
}
