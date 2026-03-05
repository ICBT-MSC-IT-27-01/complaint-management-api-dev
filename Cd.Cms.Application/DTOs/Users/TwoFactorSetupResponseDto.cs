namespace Cd.Cms.Application.DTOs.Users
{
    public sealed class TwoFactorSetupResponseDto
    {
        public string Secret { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
        public string DemoVerificationCode { get; set; } = string.Empty;
    }
}
