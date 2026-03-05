using Cd.Cms.Application.DTOs.Permissions;
using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/role-permissions")]
    [Authorize(Roles = "Admin")]
    public sealed class RolePermissionsController : ControllerBase
    {
        private static readonly Dictionary<string, List<RolePermissionDto>> Store = new(StringComparer.OrdinalIgnoreCase);
        private static readonly List<object> AuditTrail = new();

        [HttpGet("{role}")]
        public IActionResult GetByRole(string role)
        {
            Store.TryGetValue(role, out var permissions);
            return Ok(ApiResponse<object>.Success("Permissions loaded.", permissions ?? new List<RolePermissionDto>()));
        }

        [HttpPost("save")]
        public IActionResult Save([FromBody] SaveRolePermissionsRequest request)
        {
            if (request == null) return BadRequest(ApiResponse<object>.ValidationError("Request body is required."));
            if (string.IsNullOrWhiteSpace(request.Role)) return BadRequest(ApiResponse<object>.ValidationError("Role is required."));

            var validLevels = new[] { "None", "Read", "Write", "Delete", "Advanced" };
            if (request.Permissions.Any(p => !validLevels.Contains(p.PermissionLevel, StringComparer.OrdinalIgnoreCase)))
                return BadRequest(ApiResponse<object>.ValidationError("Invalid permission level."));

            Store[request.Role] = request.Permissions.Select(p => new RolePermissionDto
            {
                Role = request.Role,
                Module = p.Module,
                PermissionLevel = p.PermissionLevel
            }).ToList();

            AuditTrail.Add(new { Action = "SavePermissions", Role = request.Role, TimestampUtc = DateTime.UtcNow, Count = request.Permissions.Count });
            return Ok(ApiResponse<object>.Success("Permissions saved."));
        }

        [HttpPost("{role}/duplicate/{newRole}")]
        public IActionResult DuplicateRole(string role, string newRole)
        {
            if (!Store.TryGetValue(role, out var source))
                return NotFound(ApiResponse<object>.NotFound());

            Store[newRole] = source.Select(p => new RolePermissionDto
            {
                Role = newRole,
                Module = p.Module,
                PermissionLevel = p.PermissionLevel
            }).ToList();

            AuditTrail.Add(new { Action = "DuplicateRole", Role = role, NewRole = newRole, TimestampUtc = DateTime.UtcNow });
            return Ok(ApiResponse<object>.Success("Role duplicated."));
        }

        [HttpGet("audit-trail")]
        public IActionResult GetAuditTrail()
        {
            return Ok(ApiResponse<object>.Success("Audit trail loaded.", AuditTrail));
        }
    }
}
