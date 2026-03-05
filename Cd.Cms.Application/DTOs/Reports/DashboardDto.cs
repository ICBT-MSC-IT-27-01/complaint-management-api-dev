namespace Cd.Cms.Application.DTOs.Reports
{
    public class DashboardDto
    {
        public int TotalComplaints { get; set; }
        public int OpenComplaints { get; set; }
        public int ResolvedToday { get; set; }
        public int SlaBreached { get; set; }
        public int SlaAtRisk { get; set; }
        public int MyOpenComplaints { get; set; }
        public double SlaCompliancePercent { get; set; }
        public double AvgResolutionHours { get; set; }
        public List<StatusCountDto> ByStatus { get; set; } = new();
        public List<PriorityCountDto> ByPriority { get; set; } = new();
    }

    public class StatusCountDto { public string Status { get; set; } = string.Empty; public int Count { get; set; } }
    public class PriorityCountDto { public string Priority { get; set; } = string.Empty; public int Count { get; set; } }
}
