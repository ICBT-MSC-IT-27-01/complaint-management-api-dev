namespace Cd.Cms.Application.DTOs.Users
{
    public sealed class UserSessionDto
    {
        public string SessionId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public DateTime IssuedAtUtc { get; set; }
        public DateTime LastSeenAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public bool IsActive { get; set; }
    }
}
