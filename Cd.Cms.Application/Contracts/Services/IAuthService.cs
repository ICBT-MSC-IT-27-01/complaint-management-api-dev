using Cd.Cms.Application.DTOs.Users;

namespace Cd.Cms.Application.Contracts.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request, string? clientIp = null, string? userAgent = null, CancellationToken ct = default);
        Task<ClientEmailCheckResponseDto> CheckClientEmailAsync(string email, CancellationToken ct = default);
        Task<AuthResponseDto> RegisterClientAsync(ClientRegisterRequestDto request, string? clientIp = null, string? userAgent = null, CancellationToken ct = default);
        Task<ForgotPasswordResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, CancellationToken ct = default);
        Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken ct = default);
        Task<TwoFactorSetupResponseDto> BeginTwoFactorSetupAsync(long userId, CancellationToken ct = default);
        Task EnableTwoFactorAsync(long userId, EnableTwoFactorRequestDto request, CancellationToken ct = default);
        Task<List<UserSessionDto>> GetSessionsAsync(long userId, CancellationToken ct = default);
        Task RevokeSessionAsync(long userId, string sessionId, CancellationToken ct = default);
    }
}
