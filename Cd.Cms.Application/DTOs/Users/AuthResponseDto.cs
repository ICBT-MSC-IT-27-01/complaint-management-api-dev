namespace Cd.Cms.Application.DTOs.Users
{
    public class AuthResponseDto
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public bool TwoFactorEnabled { get; set; }
    }
}
