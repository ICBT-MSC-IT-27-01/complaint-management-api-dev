using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Complaints;
using Cd.Cms.Application.DTOs.Reports;
using Cd.Cms.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace Cd.Cms.Api.Controllers
{
    [ApiController]
    [Route("api/v1/reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _svc;
        private readonly IComplaintService _complaints;
        public ReportsController(IReportService svc, IComplaintService complaints)
        {
            _svc = svc;
            _complaints = complaints;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard([FromQuery] DashboardRequest req)
        {
            var result = await _svc.GetDashboardAsync(GetActorUserId(), GetActorRole(), req ?? new());
            return Ok(ApiResponse<object>.Success("Dashboard loaded.", result));
        }

        [HttpGet("complaints-summary")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> ComplaintSummary([FromQuery] ReportFilterRequest req)
        {
            var result = await _svc.GetComplaintSummaryAsync(req ?? new());
            return Ok(ApiResponse<object>.Success("Summary loaded.", result));
        }

        [HttpGet("high-priority-overdue")]
        public async Task<IActionResult> HighPriorityOverdue([FromQuery] int limit = 100)
        {
            var data = await _complaints.SearchAsync(new ComplaintSearchRequest
            {
                Priority = "Critical",
                Page = 1,
                PageSize = Math.Clamp(limit, 1, 500)
            });

            var now = DateTime.UtcNow;
            var result = data.Items
                .Where(x => string.Equals(x.Priority, "Critical", StringComparison.OrdinalIgnoreCase))
                .Where(x => string.Equals(x.SlaStatus, "Breached", StringComparison.OrdinalIgnoreCase) || (x.DueDate.HasValue && x.DueDate.Value < now))
                .Select(x => new OverdueCaseDto
                {
                    ComplaintId = x.Id,
                    ComplaintNumber = x.ComplaintNumber,
                    Subject = x.Subject,
                    Priority = x.Priority,
                    Status = x.Status,
                    AssignedToName = x.AssignedToName,
                    DueDate = x.DueDate,
                    IsOverdue = string.Equals(x.SlaStatus, "Breached", StringComparison.OrdinalIgnoreCase) || (x.DueDate.HasValue && x.DueDate.Value < now)
                })
                .ToList();

            return Ok(ApiResponse<object>.Success("High priority overdue complaints loaded.", result));
        }

        [HttpGet("complaints-summary/export/pdf")]
        [Authorize(Roles = "Admin,Supervisor")]
        public async Task<IActionResult> ExportComplaintSummaryPdf([FromQuery] ReportFilterRequest req)
        {
            var summary = await _svc.GetComplaintSummaryAsync(req ?? new());
            var content = $"Complaint Summary{Environment.NewLine}" +
                          $"Total: {summary.TotalComplaints}{Environment.NewLine}" +
                          $"New: {summary.NewCount}{Environment.NewLine}" +
                          $"InProgress: {summary.InProgressCount}{Environment.NewLine}" +
                          $"Resolved: {summary.ResolvedCount}{Environment.NewLine}" +
                          $"Closed: {summary.ClosedCount}{Environment.NewLine}" +
                          $"Escalated: {summary.EscalatedCount}{Environment.NewLine}" +
                          $"SLA Breached: {summary.SlaBreachedCount}{Environment.NewLine}";

            var bytes = BuildSimplePdf(content);
            return File(bytes, "application/pdf", $"complaints-summary-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }

        private long GetActorUserId() => long.Parse(User.FindFirst("uid")?.Value ?? "0");
        private string GetActorRole() => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

        private static byte[] BuildSimplePdf(string text)
        {
            var safeText = text.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
            var stream = $"BT /F1 12 Tf 50 760 Td ({safeText}) Tj ET";
            var pdf = new StringBuilder();
            pdf.Append("%PDF-1.4\n");
            pdf.Append("1 0 obj<< /Type /Catalog /Pages 2 0 R >>endobj\n");
            pdf.Append("2 0 obj<< /Type /Pages /Kids [3 0 R] /Count 1 >>endobj\n");
            pdf.Append("3 0 obj<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources<< /Font<< /F1 4 0 R >> >> /Contents 5 0 R >>endobj\n");
            pdf.Append("4 0 obj<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>endobj\n");
            pdf.Append($"5 0 obj<< /Length {stream.Length} >>stream\n{stream}\nendstream endobj\n");
            pdf.Append("xref\n0 6\n0000000000 65535 f \n");
            pdf.Append("0000000010 00000 n \n0000000060 00000 n \n0000000117 00000 n \n0000000243 00000 n \n0000000313 00000 n \n");
            pdf.Append("trailer<< /Root 1 0 R /Size 6 >>\nstartxref\n420\n%%EOF");
            return Encoding.ASCII.GetBytes(pdf.ToString());
        }
    }
}
