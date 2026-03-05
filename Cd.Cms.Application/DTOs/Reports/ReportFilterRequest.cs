namespace Cd.Cms.Application.DTOs.Reports
{
    public class ReportFilterRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public long? AgentUserId { get; set; }
        public long? CategoryId { get; set; }
        public string? Department { get; set; }
    }
}
