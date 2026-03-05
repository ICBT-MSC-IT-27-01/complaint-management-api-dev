namespace Cd.Cms.Application.DTOs.Reports
{
    public sealed class OverdueCaseDto
    {
        public long ComplaintId { get; set; }
        public string ComplaintNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public bool IsOverdue { get; set; }
    }
}
