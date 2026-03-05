using Cd.Cms.Application.DTOs.Teams;
using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/teams")]
    [Authorize(Roles = "Admin,Supervisor")]
    public sealed class TeamsController : ControllerBase
    {
        private static readonly List<TeamDto> Teams = new();
        private static long _nextId = 1;

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(ApiResponse<object>.Success("Teams loaded.", Teams));
        }

        [HttpPost]
        public IActionResult Create([FromBody] CreateTeamRequest dto)
        {
            if (dto == null) return BadRequest(ApiResponse<object>.ValidationError("Request body is required."));
            if (string.IsNullOrWhiteSpace(dto.TeamName)) return BadRequest(ApiResponse<object>.ValidationError("Team name is required."));
            if (dto.PrimaryLeadUserId <= 0) return BadRequest(ApiResponse<object>.ValidationError("Primary lead is required."));
            if (string.IsNullOrWhiteSpace(dto.Department)) return BadRequest(ApiResponse<object>.ValidationError("Department is required."));

            var team = new TeamDto
            {
                Id = _nextId++,
                TeamName = dto.TeamName.Trim(),
                PrimaryLeadUserId = dto.PrimaryLeadUserId,
                Department = dto.Department.Trim(),
                CreatedAtUtc = DateTime.UtcNow,
                IsArchived = false
            };
            Teams.Add(team);
            return StatusCode(201, ApiResponse<object>.Success("Team created.", team));
        }

        [HttpPatch("{id:long}/archive")]
        public IActionResult Archive(long id)
        {
            var team = Teams.FirstOrDefault(t => t.Id == id);
            if (team == null) return NotFound(ApiResponse<object>.NotFound());
            team.IsArchived = true;
            return Ok(ApiResponse<object>.Success("Team archived."));
        }
    }
}
