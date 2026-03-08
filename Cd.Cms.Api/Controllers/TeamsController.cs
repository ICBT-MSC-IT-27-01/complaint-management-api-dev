using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Teams;
using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/teams")]
    [Authorize(Roles = "Admin")]
    public sealed class TeamsController : ControllerBase
    {
        private readonly ITeamService _svc;
        public TeamsController(ITeamService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] TeamSearchRequest req)
        {
            var result = await _svc.SearchAsync(req ?? new());
            return Ok(ApiResponse<object>.Success("Teams loaded.", result));
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _svc.GetByIdAsync(id);
            return result == null ? NotFound(ApiResponse<object>.NotFound()) : Ok(ApiResponse<object>.Success("Team loaded.", result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeamRequest dto)
        {
            try
            {
                var result = await _svc.CreateAsync(dto, GetActorUserId());
                return StatusCode(201, ApiResponse<object>.Success("Team created.", result));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateTeamRequest dto)
        {
            try
            {
                await _svc.UpdateAsync(id, dto, GetActorUserId());
                return Ok(ApiResponse<object>.Success("Team updated."));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPost("{id:long}/members")]
        public async Task<IActionResult> AddMember(long id, [FromBody] AssignTeamMemberRequest dto)
        {
            try
            {
                await _svc.AddMemberAsync(id, dto.UserId, GetActorUserId());
                return Ok(ApiResponse<object>.Success("Team member assigned."));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpDelete("{id:long}/members/{userId:long}")]
        public async Task<IActionResult> RemoveMember(long id, long userId)
        {
            try
            {
                await _svc.RemoveMemberAsync(id, userId, GetActorUserId());
                return Ok(ApiResponse<object>.Success("Team member removed."));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _svc.DeleteAsync(id, GetActorUserId());
                return Ok(ApiResponse<object>.Success("Team deleted."));
            }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        private long GetActorUserId() => long.Parse(User.FindFirst("uid")?.Value ?? "0");
    }
}
