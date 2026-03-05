using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Users;
using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public sealed class UsersController : ControllerBase
    {
        private readonly IUserService _users;
        public UsersController(IUserService users) => _users = users;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Search([FromQuery] UserSearchRequest req)
        {
            var result = await _users.SearchAsync(req ?? new());
            return Ok(ApiResponse<object>.Success("Users loaded.", result));
        }

        [HttpGet("{id:long}")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _users.GetByIdAsync(id);
            return result == null ? NotFound(ApiResponse<object>.NotFound()) : Ok(ApiResponse<object>.Success("User loaded.", result));
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var result = await _users.GetByIdAsync(GetActorUserId());
            return result == null ? NotFound(ApiResponse<object>.NotFound()) : Ok(ApiResponse<object>.Success("Profile loaded.", result));
        }

        [HttpGet("agents")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> GetAgents()
        {
            var result = await _users.GetAgentsAsync();
            return Ok(ApiResponse<object>.Success("Agents loaded.", result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest dto)
        {
            try
            {
                var result = await _users.CreateAsync(dto, GetActorUserId());
                return StatusCode(201, ApiResponse<object>.Success("User created.", result));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex)         { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPut("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest dto)
        {
            try { await _users.UpdateAsync(id, dto, GetActorUserId()); return Ok(ApiResponse<object>.Success("User updated.")); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPut("me/change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto)
        {
            try { await _users.ChangePasswordAsync(GetActorUserId(), dto, GetActorUserId()); return Ok(ApiResponse<object>.Success("Password changed.")); }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex)         { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                if (id == GetActorUserId())
                    return BadRequest(ApiResponse<object>.ValidationError("You cannot delete your own account."));

                await _users.DeleteAsync(id, GetActorUserId());
                return Ok(ApiResponse<object>.Success("User deleted."));
            }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpGet("export/csv")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ExportCsv([FromQuery] UserSearchRequest req)
        {
            var data = await _users.SearchAsync(req ?? new());
            var sb = new StringBuilder();
            sb.AppendLine("Id,Name,Email,Username,Role,Department,IsActive,LastLoginDateTime");
            foreach (var u in data.Items)
            {
                sb.AppendLine($"{u.Id},\"{Escape(u.Name)}\",{u.Email},{u.Username},{u.Role},\"{Escape(u.Department)}\",{u.IsActive},{u.LastLoginDateTime:O}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"users-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }

        private long GetActorUserId() => long.Parse(User.FindFirst("uid")?.Value ?? "0");
        private static string Escape(string value) => value.Replace("\"", "\"\"");
    }
}
