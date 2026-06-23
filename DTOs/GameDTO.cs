namespace Esportify.DTOs
{
    public class GameDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Genre { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? OfficialWebsite { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}