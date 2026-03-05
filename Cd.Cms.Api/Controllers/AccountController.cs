using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Users;
using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/account")]
    [Authorize]
    public sealed class AccountController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IUserService _users;

        public AccountController(IAuthService auth, IUserService users)
        {
            _auth = auth;
            _users = users;
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions(CancellationToken ct)
        {
            var result = await _auth.GetSessionsAsync(GetActorUserId(), ct);
            return Ok(ApiResponse<object>.Success("Sessions loaded.", result));
        }

        [HttpDelete("sessions/{sessionId}")]
        public async Task<IActionResult> RevokeSession(string sessionId, CancellationToken ct)
        {
            try
            {
                await _auth.RevokeSessionAsync(GetActorUserId(), sessionId, ct);
                return Ok(ApiResponse<object>.Success("Session revoked."));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (InvalidOperationException ex) { return NotFound(ApiResponse<object>.Error(ex.Message, ResponseCodes.NOT_FOUND)); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPost("2fa/setup")]
        public async Task<IActionResult> BeginTwoFactorSetup(CancellationToken ct)
        {
            var result = await _auth.BeginTwoFactorSetupAsync(GetActorUserId(), ct);
            return Ok(ApiResponse<object>.Success("2FA setup initialized.", result));
        }

        [HttpPost("2fa/enable")]
        public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequestDto dto, CancellationToken ct)
        {
            try
            {
                if (dto == null) return BadRequest(ApiResponse<object>.ValidationError("Request body is required."));
                await _auth.EnableTwoFactorAsync(GetActorUserId(), dto, ct);
                return Ok(ApiResponse<object>.Success("2FA enabled."));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(ApiResponse<object>.Unauthorized(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Error(ex.Message, ResponseCodes.BAD_REQUEST)); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpDelete("deactivate")]
        public async Task<IActionResult> DeactivateOwnAccount()
        {
            try
            {
                var actor = GetActorUserId();
                await _users.DeleteAsync(actor, actor);
                return Ok(ApiResponse<object>.Success("Account deactivated."));
            }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        private long GetActorUserId() => long.Parse(User.FindFirst("uid")?.Value ?? "0");
    }
}
