using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Users;
using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public sealed class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
        {
            try
            {
                if (dto == null) return BadRequest(ApiResponse<object>.ValidationError("Request body is required."));

                if (string.IsNullOrWhiteSpace(dto.Password))
                {
                    var check = await _auth.CheckClientEmailAsync(dto.EmailOrUsername, ct);
                    return Ok(ApiResponse<object>.Success("Email check completed.", check));
                }

                var result = await _auth.LoginAsync(dto, GetClientIpAddress(), GetUserAgent(), ct);
                return Ok(ApiResponse<object>.Success("Login successful.", result));
            }
            catch (ArgumentException ex)          { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (UnauthorizedAccessException ex) { return Unauthorized(ApiResponse<object>.Unauthorized(ex.Message)); }
            catch (InvalidOperationException ex)   { return BadRequest(ApiResponse<object>.Error(ex.Message, ResponseCodes.BAD_REQUEST)); }
            catch (Exception ex)                   { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPost("client/register")]
        public async Task<IActionResult> RegisterClient([FromBody] ClientRegisterRequestDto dto, CancellationToken ct)
        {
            try
            {
                if (dto == null) return BadRequest(ApiResponse<object>.ValidationError("Request body is required."));
                var result = await _auth.RegisterClientAsync(dto, GetClientIpAddress(), GetUserAgent(), ct);
                return StatusCode(201, ApiResponse<object>.Success("Client account created.", result));
            }
            catch (ArgumentException ex)          { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (InvalidOperationException ex)   { return BadRequest(ApiResponse<object>.Error(ex.Message, ResponseCodes.BAD_REQUEST)); }
            catch (Exception ex)                   { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto, CancellationToken ct)
        {
            try
            {
                if (dto == null) return BadRequest(ApiResponse<object>.ValidationError("Request body is required."));
                var result = await _auth.ForgotPasswordAsync(dto, ct);
                return Ok(ApiResponse<object>.Success("Temporary password request accepted.", result));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex)         { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto, CancellationToken ct)
        {
            try
            {
                if (dto == null) return BadRequest(ApiResponse<object>.ValidationError("Request body is required."));
                await _auth.ResetPasswordAsync(dto, ct);
                return Ok(ApiResponse<object>.Success("Password reset completed."));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (InvalidOperationException ex) { return BadRequest(ApiResponse<object>.Error(ex.Message, ResponseCodes.BAD_REQUEST)); }
            catch (Exception ex)         { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        private string GetClientIpAddress()
        {
            var forwarded = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(forwarded))
                return forwarded.Split(',')[0].Trim();

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
        }

        private string GetUserAgent()
        {
            var userAgent = Request.Headers.UserAgent.ToString();
            return string.IsNullOrWhiteSpace(userAgent) ? "Unknown Device" : userAgent;
        }
    }
}
