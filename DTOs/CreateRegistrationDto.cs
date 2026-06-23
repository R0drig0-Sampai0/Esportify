using System.ComponentModel.DataAnnotations;

namespace Esportify.DTOs
{
    public class CreateRegistrationDto
    {
        [Required(ErrorMessage = "O identificador da equipa é obrigatório.")]
        public string TeamId { get; set; } = null!;
    }
}
