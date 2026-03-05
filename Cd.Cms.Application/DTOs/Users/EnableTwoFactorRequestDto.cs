namespace Cd.Cms.Application.DTOs.Users
{
    public sealed class EnableTwoFactorRequestDto
    {
        public string VerificationCode { get; set; } = string.Empty;
    }
}
