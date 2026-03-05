namespace Cd.Cms.Application.DTOs.Users
{
    public sealed class ForgotPasswordResponseDto
    {
        public bool RequestAccepted { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? DemoResetToken { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
    }
}
