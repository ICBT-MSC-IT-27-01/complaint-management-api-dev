namespace Cd.Cms.Application.DTOs.Complaints
{
    public sealed class SlaTimerDto
    {
        public DateTime? DueDate { get; set; }
        public TimeSpan? Remaining { get; set; }
        public bool IsOverdue { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
