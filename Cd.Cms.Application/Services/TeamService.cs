using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.Contracts.Services;
using Cd.Cms.Application.DTOs.Teams;
using Cd.Cms.Shared;

namespace Cd.Cms.Application.Services
{
    public sealed class TeamService : ITeamService
    {
        private readonly ITeamRepository _repo;
        public TeamService(ITeamRepository repo) => _repo = repo;

        public Task<TeamDto?> GetByIdAsync(long id) => _repo.GetByIdAsync(id);
        public Task<PagedResult<TeamDto>> SearchAsync(TeamSearchRequest request) => _repo.SearchAsync(request);

        public Task<TeamDto> CreateAsync(CreateTeamRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.TeamName)) throw new ArgumentException("Team name is required.");
            return _repo.CreateAsync(request, actorUserId);
        }

        public Task UpdateAsync(long id, UpdateTeamRequest request, long actorUserId)
        {
            if (string.IsNullOrWhiteSpace(request.TeamName)) throw new ArgumentException("Team name is required.");
            return _repo.UpdateAsync(id, request, actorUserId);
        }

        public Task DeleteAsync(long id, long actorUserId) => _repo.DeleteAsync(id, actorUserId);

        public Task AddMemberAsync(long teamId, long userId, long actorUserId)
        {
            if (userId <= 0) throw new ArgumentException("User id is required.");
            return _repo.AddMemberAsync(teamId, userId, actorUserId);
        }

        public Task RemoveMemberAsync(long teamId, long userId, long actorUserId)
        {
            if (userId <= 0) throw new ArgumentException("User id is required.");
            return _repo.RemoveMemberAsync(teamId, userId, actorUserId);
        }
    }
}
