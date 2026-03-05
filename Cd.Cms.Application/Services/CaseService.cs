using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Cases;

namespace Cd.Cms.Application.Services
{
    public class CaseService : ICaseService
    {
        private readonly ICaseRepository _repo;
        public CaseService(ICaseRepository repo) => _repo = repo;

        public Task<CaseDto?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);
        public Task<CaseDto?> GetByComplaintIdAsync(long complaintId) => _repo.GetByComplaintIdAsync(complaintId);
        public Task<List<CaseActivityDto>> GetActivitiesAsync(long caseId) => _repo.GetActivitiesAsync(caseId);
        public Task AddActivityAsync(long caseId, AddCaseActivityRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Description)) throw new ArgumentException("Description is required.");
            if (!string.Equals(request.Visibility, "Public", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(request.Visibility, "Private", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Visibility must be Public or Private.");
            }

            var visibility = string.Equals(request.Visibility, "Public", StringComparison.OrdinalIgnoreCase) ? "Public" : "Private";
            var notify = request.NotifyClient ? "NotifyClient" : "NoNotify";
            request.Description = $"[{visibility}|{notify}] {request.Description.Trim()}";
            return _repo.AddActivityAsync(caseId, request, actorUserId);
        }
        public Task UpdateStatusAsync(long caseId, UpdateCaseStatusRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Status)) throw new ArgumentException("Status is required.");
            return _repo.UpdateStatusAsync(caseId, request, actorUserId);
        }
    }
}
