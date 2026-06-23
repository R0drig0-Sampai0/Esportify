using System.ComponentModel.DataAnnotations;

namespace Esportify.DTOs
{
    public class UpdateTeamDto
    {
        [Required]
        [StringLength(64)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(10)]
        public string? Tag { get; set; }

        [Url]
        public string? LogoUrl { get; set; }

        public bool IsOpenForMembers { get; set; } = false;
    }
}
