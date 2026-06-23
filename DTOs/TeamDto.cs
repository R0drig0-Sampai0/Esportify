using System;

namespace Esportify.DTOs
{
    public class TeamDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Tag { get; set; }
        public string? LogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsOpenForMembers { get; set; }
        public string LeaderId { get; set; } = string.Empty;
        public string? LeaderUserName { get; set; }
        public int MembersCount { get; set; }
    }
}
