using Cd.Cms.Application.DTOs.Reports;

namespace Cd.Cms.Application.Contracts.Repositories
{
    public interface IReportRepository
    {
        Task<DashboardDto> GetDashboardAsync(long actorUserId, string role, DashboardRequest request);
        Task<ComplaintSummaryDto> GetComplaintSummaryAsync(ReportFilterRequest request);
    }
}
