namespace Esportify.DTOs
{
    public class RegistrationDto
    {
        public string TeamId { get; set; } = null!;
        public string TeamName { get; set; } = null!;
        public string? TeamTag { get; set; }
        public DateTime RegistrationDate { get; set; }
    }
}
