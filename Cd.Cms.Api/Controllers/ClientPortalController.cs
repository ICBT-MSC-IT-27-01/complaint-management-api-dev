using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Cases;
using Cd.Cms.Application.DTOs.Complaints;
using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/client-portal")]
    [Authorize(Roles = "Client")]
    public sealed class ClientPortalController : ControllerBase
    {
        private readonly IComplaintService _complaints;
        private readonly ICaseService _cases;

        public ClientPortalController(IComplaintService complaints, ICaseService cases)
        {
            _complaints = complaints;
            _cases = cases;
        }

        [HttpPost("complaints")]
        public async Task<IActionResult> CreateComplaint([FromBody] ClientCreateComplaintRequest dto)
        {
            try
            {
                if (dto == null) return BadRequest(ApiResponse<object>.ValidationError("Request body is required."));
                var request = new CreateComplaintRequest
                {
                    ClientEmail = GetActorEmail(),
                    ComplaintCategoryId = dto.ComplaintCategoryId,
                    ComplaintChannelId = dto.ComplaintChannelId,
                    Subject = dto.Subject,
                    Description = dto.Description,
                    Priority = "Medium"
                };

                var result = await _complaints.CreateAsync(request, GetActorUserId());
                return StatusCode(201, ApiResponse<object>.Success("Complaint filed.", result));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpGet("complaints")]
        public async Task<IActionResult> MyComplaints([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var search = await _complaints.SearchAsync(new ComplaintSearchRequest
            {
                Page = Math.Max(page, 1),
                PageSize = Math.Clamp(pageSize, 1, 200),
            });

            var actorEmail = GetActorEmail();
            var filtered = new List<ComplaintListItemDto>();
            foreach (var item in search.Items)
            {
                var full = await _complaints.GetByIdAsync(item.Id);
                if (full != null && string.Equals(full.ClientEmail, actorEmail, StringComparison.OrdinalIgnoreCase))
                    filtered.Add(item);
            }

            return Ok(ApiResponse<object>.Success("My complaints loaded.", filtered));
        }

        [HttpPost("complaints/{id:long}/reply")]
        public async Task<IActionResult> Reply(long id, [FromBody] AddCaseActivityRequest dto)
        {
            try
            {
                if (dto == null) return BadRequest(ApiResponse<object>.ValidationError("Request body is required."));

                var complaint = await _complaints.GetByIdAsync(id);
                if (complaint == null) return NotFound(ApiResponse<object>.NotFound());

                var caseEntity = await _cases.GetByComplaintIdAsync(id);
                if (caseEntity == null) return NotFound(ApiResponse<object>.Error("Case not found.", ResponseCodes.NOT_FOUND));

                dto.ActivityType = "PublicReply";
                dto.Visibility = "Public";
                dto.NotifyClient = true;

                await _cases.AddActivityAsync(caseEntity.Id, dto, GetActorUserId());
                return Ok(ApiResponse<object>.Success("Reply added."));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        private long GetActorUserId() => long.Parse(User.FindFirst("uid")?.Value ?? "0");
        private string GetActorEmail() => User.FindFirst(JwtRegisteredClaimNames.Email)?.Value ?? string.Empty;
    }
}
