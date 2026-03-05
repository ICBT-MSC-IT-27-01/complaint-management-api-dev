namespace Cd.Cms.Application.DTOs.Complaints
{
    public class EscalateComplaintRequest
    {
        public string Reason { get; set; } = string.Empty;
        public long? EscalatedToUserId { get; set; }
        public string EscalationType { get; set; } = "Manual";
    }
}
