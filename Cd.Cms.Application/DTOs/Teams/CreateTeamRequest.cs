namespace Cd.Cms.Application.DTOs.Teams
{
    public sealed class CreateTeamRequest
    {
        public string TeamName { get; set; } = string.Empty;
        public long? LeadUserId { get; set; }
    }
}
