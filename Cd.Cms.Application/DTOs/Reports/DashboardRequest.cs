namespace Cd.Cms.Application.DTOs.Reports
{
    public sealed class DashboardRequest
    {
        public string Period { get; set; } = "30d";
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
