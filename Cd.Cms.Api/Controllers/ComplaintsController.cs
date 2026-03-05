using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Complaints;
using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/complaints")]
    [Authorize]
    public class ComplaintsController : ControllerBase
    {
        private readonly IComplaintService _svc;
        public ComplaintsController(IComplaintService svc) => _svc = svc;

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] ComplaintSearchRequest req)
        {
            try
            {
                var result = await _svc.SearchAsync(req ?? new());
                return Ok(ApiResponse<object>.Success("Complaints loaded.", result));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _svc.GetByIdAsync(id);
            return result == null ? NotFound(ApiResponse<object>.NotFound()) : Ok(ApiResponse<object>.Success("Complaint loaded.", result));
        }

        [HttpGet("{id:long}/history")]
        public async Task<IActionResult> GetHistory(long id)
        {
            var result = await _svc.GetHistoryAsync(id);
            return Ok(ApiResponse<object>.Success("History loaded.", result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Supervisor,Agent")]
        public async Task<IActionResult> Create([FromBody] CreateComplaintRequest dto)
        {
            try
            {
                var result = await _svc.CreateAsync(dto, GetActorUserId());
                return StatusCode(201, ApiResponse<object>.Success("Complaint created.", result));
            }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex)         { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPatch("{id:long}/status")]
        [Authorize(Roles = "Admin,Supervisor,Agent")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateComplaintStatusRequest dto)
        {
            try { await _svc.UpdateStatusAsync(id, dto, GetActorUserId()); return Ok(ApiResponse<object>.Success("Status updated.")); }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex)         { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPost("{id:long}/assign")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Assign(long id, [FromBody] AssignComplaintRequest dto)
        {
            try { await _svc.AssignAsync(id, dto, GetActorUserId()); return Ok(ApiResponse<object>.Success("Complaint assigned.")); }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex)         { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPost("{id:long}/escalate")]
        [Authorize(Roles = "Admin,Supervisor,Agent")]
        public async Task<IActionResult> Escalate(long id, [FromBody] EscalateComplaintRequest dto)
        {
            try { await _svc.EscalateAsync(id, dto, GetActorUserId()); return Ok(ApiResponse<object>.Success("Complaint escalated.")); }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex)         { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPost("{id:long}/resolve")]
        [Authorize(Roles = "Admin,Supervisor,Agent")]
        public async Task<IActionResult> Resolve(long id, [FromBody] ResolveComplaintRequest dto)
        {
            try { await _svc.ResolveAsync(id, dto, GetActorUserId()); return Ok(ApiResponse<object>.Success("Complaint resolved.")); }
            catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.ValidationError(ex.Message)); }
            catch (Exception ex)         { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpPost("{id:long}/close")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> Close(long id)
        {
            try { await _svc.CloseAsync(id, GetActorUserId()); return Ok(ApiResponse<object>.Success("Complaint closed.")); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpDelete("{id:long}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long id)
        {
            try { await _svc.DeleteAsync(id, GetActorUserId()); return Ok(ApiResponse<object>.Success("Complaint deleted.")); }
            catch (Exception ex) { return StatusCode(500, ApiResponse<object>.Error(ex.Message)); }
        }

        [HttpGet("{id:long}/sla-timer")]
        public async Task<IActionResult> GetSlaTimer(long id)
        {
            var complaint = await _svc.GetByIdAsync(id);
            if (complaint == null) return NotFound(ApiResponse<object>.NotFound());

            var now = DateTime.UtcNow;
            var isOverdue = complaint.DueDate.HasValue && complaint.DueDate.Value <= now;
            var dto = new SlaTimerDto
            {
                DueDate = complaint.DueDate,
                Remaining = complaint.DueDate.HasValue ? complaint.DueDate.Value - now : null,
                IsOverdue = isOverdue || complaint.IsSlaBreached,
                Status = isOverdue || complaint.IsSlaBreached ? "Overdue" : "WithinSLA"
            };

            return Ok(ApiResponse<object>.Success("SLA timer loaded.", dto));
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportCsv([FromQuery] ComplaintSearchRequest req)
        {
            var result = await _svc.SearchAsync(req ?? new());
            var sb = new StringBuilder();
            sb.AppendLine("Id,ComplaintNumber,Subject,Priority,Status,Category,ClientName,AssignedToName,SlaStatus,DueDate,CreatedDateTime");
            foreach (var c in result.Items)
            {
                sb.AppendLine($"{c.Id},{c.ComplaintNumber},\"{Escape(c.Subject)}\",{c.Priority},{c.Status},\"{Escape(c.Category)}\",\"{Escape(c.ClientName)}\",\"{Escape(c.AssignedToName)}\",{c.SlaStatus},{c.DueDate:O},{c.CreatedDateTime:O}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", $"complaints-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }

        private long GetActorUserId() => long.Parse(User.FindFirst("uid")?.Value ?? "0");
        private static string Escape(string value) => value.Replace("\"", "\"\"");
    }
}
