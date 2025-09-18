using System.ComponentModel.DataAnnotations;

namespace Esportify.Models.ViewModels
{
    public class TeamViewModel
    {
        [Required(ErrorMessage = "O nome da equipa é obrigatório.")]
        [StringLength(50, ErrorMessage = "O nome da equipa não pode ter mais de 50 caracteres.")]
        [Display(Name = "Nome da Equipa")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "A descrição não pode ter mais de 200 caracteres.")]
        [Display(Name = "Descrição")]
        public string? Description { get; set; }
        public IFormFile Image { get; set; }
    }
}
