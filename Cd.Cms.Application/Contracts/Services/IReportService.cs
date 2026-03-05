using Cd.Cms.Application.DTOs.Reports;

namespace Cd.Cms.Application.Contracts.Services
{
    public interface IReportService
    {
        Task<DashboardDto> GetDashboardAsync(long actorUserId, string role, DashboardRequest request);
        Task<ComplaintSummaryDto> GetComplaintSummaryAsync(ReportFilterRequest request);
    }
}
