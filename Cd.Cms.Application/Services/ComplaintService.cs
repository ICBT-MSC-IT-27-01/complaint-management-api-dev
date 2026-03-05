using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Complaints;
using Cd.Cms.Shared;
using System.Text.RegularExpressions;

namespace Cd.Cms.Application.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _repo;
        private readonly IUserRepository _users;

        public ComplaintService(IComplaintRepository repo, IUserRepository users)
        {
            _repo = repo;
            _users = users;
        }

        public Task<ComplaintDto?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);
        public Task<PagedResult<ComplaintListItemDto>> SearchAsync(ComplaintSearchRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.Q) && request.Q.Trim().Length < 2)
                throw new ArgumentException("Search query must be at least 2 characters.");
            return _repo.SearchAsync(request);
        }

        public Task<ComplaintDto> CreateAsync(CreateComplaintRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Subject)) throw new ArgumentException("Subject is required.");
            if (string.IsNullOrWhiteSpace(request.Description)) throw new ArgumentException("Description is required.");
            if (string.IsNullOrWhiteSpace(request.Priority)) throw new ArgumentException("Priority is required.");
            if (request.Subject.Trim().Length < 5) throw new ArgumentException("Subject must be at least 5 characters.");
            if (request.Description.Trim().Length < 20) throw new ArgumentException("Description must be at least 20 characters.");
            if (request.Subject.Length > 300) throw new ArgumentException("Subject cannot exceed 300 characters.");
            if (request.ClientName?.Length > 100) throw new ArgumentException("Client name cannot exceed 100 characters.");
            if (!string.IsNullOrWhiteSpace(request.ClientEmail) && !Regex.IsMatch(request.ClientEmail, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Client email is invalid.");
            var allowed = new[] { "Low", "Medium", "High", "Critical" };
            if (!allowed.Contains(request.Priority, StringComparer.OrdinalIgnoreCase))
                throw new ArgumentException("Priority must be one of Low, Medium, High, Critical.");
            return _repo.CreateAsync(request, actorUserId);
        }

        public Task AssignAsync(long id, AssignComplaintRequest request, long actorUserId)
        {
            if (request.AssignedToUserId <= 0) throw new ArgumentException("AssignedToUserId is required.");
            return _repo.AssignAsync(id, request, actorUserId);
        }

        public Task UpdateStatusAsync(long id, UpdateComplaintStatusRequest request, long actorUserId)
        {
            if (request.StatusId <= 0) throw new ArgumentException("StatusId is required.");
            return _repo.UpdateStatusAsync(id, request, actorUserId);
        }

        public async Task EscalateAsync(long id, EscalateComplaintRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Reason)) throw new ArgumentException("Escalation reason is required.");
            if (!request.EscalatedToUserId.HasValue || request.EscalatedToUserId <= 0)
            {
                var agents = await _users.GetAgentsAsync();
                var target = agents.FirstOrDefault(x => string.Equals(x.Role, "Supervisor", StringComparison.OrdinalIgnoreCase))
                             ?? agents.FirstOrDefault();
                if (target == null) throw new ArgumentException("No escalation target user available.");
                request.EscalatedToUserId = target.Id;
                request.EscalationType = "Auto";
            }
            await _repo.EscalateAsync(id, request, actorUserId);
        }

        public async Task ResolveAsync(long id, ResolveComplaintRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.ResolutionSummary)) throw new ArgumentException("Resolution summary is required.");
            var history = await _repo.GetHistoryAsync(id);
            var hasResponse = history.Any(h => !string.Equals(h.Action, "Create", StringComparison.OrdinalIgnoreCase));
            if (!hasResponse) throw new ArgumentException("Cannot resolve complaint without at least one response/activity.");
            await _repo.ResolveAsync(id, request, actorUserId);
        }

        public Task CloseAsync(long id, long actorUserId) => _repo.CloseAsync(id, actorUserId);
        public Task DeleteAsync(long id, long actorUserId) => _repo.DeleteAsync(id, actorUserId);
        public Task<List<ComplaintHistoryDto>> GetHistoryAsync(long id) => _repo.GetHistoryAsync(id);
    }
}
