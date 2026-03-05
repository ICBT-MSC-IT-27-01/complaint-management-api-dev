namespace Cd.Cms.Application.DTOs.Users
{
    public class LoginRequestDto
    {
        public string EmailOrUsername { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? DeviceId { get; set; }
        public string? TwoFactorCode { get; set; }
    }
}
