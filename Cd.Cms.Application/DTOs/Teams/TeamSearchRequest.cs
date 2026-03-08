namespace Cd.Cms.Application.DTOs.Teams
{
    public sealed class TeamSearchRequest
    {
        public string? Q { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
