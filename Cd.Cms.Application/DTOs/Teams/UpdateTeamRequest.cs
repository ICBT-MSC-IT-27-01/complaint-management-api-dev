namespace Cd.Cms.Application.DTOs.Teams
{
    public sealed class UpdateTeamRequest
    {
        public string TeamName { get; set; } = string.Empty;
        public long? LeadUserId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
