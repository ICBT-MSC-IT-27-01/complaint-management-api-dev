namespace Cd.Cms.Application.DTOs.Complaints
{
    public class ComplaintSearchRequest
    {
        public long? StatusId { get; set; }
        public long? CategoryId { get; set; }
        public long? ChannelId { get; set; }
        public string? Department { get; set; }
        public string? Priority { get; set; }
        public long? AssignedToUserId { get; set; }
        public string? Q { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
