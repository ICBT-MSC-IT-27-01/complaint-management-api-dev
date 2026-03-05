namespace Cd.Cms.Application.DTOs.Teams
{
    public sealed class TeamDto
    {
        public long Id { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public long PrimaryLeadUserId { get; set; }
        public string Department { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
