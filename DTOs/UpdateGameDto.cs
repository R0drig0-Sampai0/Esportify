using System.ComponentModel.DataAnnotations;

namespace Esportify.DTOs
{
    public class UpdateGameDto
    {
        [Required(ErrorMessage = "O nome do jogo é obrigatório.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Genre { get; set; }

        public string? ImageUrl { get; set; }

        public string? OfficialWebsite { get; set; }
    }
}