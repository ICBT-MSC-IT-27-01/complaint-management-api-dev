using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Reports;

namespace Cd.Cms.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repo;
        public ReportService(IReportRepository repo) => _repo = repo;

        public Task<DashboardDto> GetDashboardAsync(long actorUserId, string role, DashboardRequest request) => _repo.GetDashboardAsync(actorUserId, role, request);
        public Task<ComplaintSummaryDto> GetComplaintSummaryAsync(ReportFilterRequest request) => _repo.GetComplaintSummaryAsync(request);
    }
}
