using System.ComponentModel.DataAnnotations;

namespace Esportify.Models.ViewModels
{
    public class TournamentsViewModel
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string GameId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Registration Deadline")]
        public DateTime RegistrationDeadline { get; set; }

        [Display(Name = "Max Teams")]
        public int MaxTeams { get; set; } = 16;

        [Display(Name = "Min Team Size")]
        public int MinTeamSize { get; set; } = 3;

        [Display(Name = "Max Team Size")]
        public int MaxTeamSize { get; set; } = 5;

        [Display(Name = "Prize Pool")]
        [Range(0, double.MaxValue)]
        public decimal PrizePool { get; set; } = 1000;

        public IFormFile Image { get; set; } 
    }
}
