namespace Cd.Cms.Application.DTOs.Teams
{
    public sealed class TeamDto
    {
        public long Id { get; set; }
        public string TeamCode { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public long? LeadUserId { get; set; }
        public string LeadName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int MemberCount { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public List<TeamMemberDto> Members { get; set; } = new();
    }
}
