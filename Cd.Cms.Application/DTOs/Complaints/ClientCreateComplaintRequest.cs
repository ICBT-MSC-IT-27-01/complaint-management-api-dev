namespace Cd.Cms.Application.DTOs.Complaints
{
    public sealed class ClientCreateComplaintRequest
    {
        public long ComplaintCategoryId { get; set; }
        public long ComplaintChannelId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
