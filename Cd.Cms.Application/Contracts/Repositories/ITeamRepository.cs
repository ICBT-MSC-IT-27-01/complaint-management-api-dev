using Cd.Cms.Application.DTOs.Teams;
using Cd.Cms.Shared;

namespace Cd.Cms.Application.Contracts.Repositories
{
    public interface ITeamRepository
    {
        Task<TeamDto?> GetByIdAsync(long id);
        Task<PagedResult<TeamDto>> SearchAsync(TeamSearchRequest request);
        Task<TeamDto> CreateAsync(CreateTeamRequest request, long actorUserId);
        Task UpdateAsync(long id, UpdateTeamRequest request, long actorUserId);
        Task DeleteAsync(long id, long actorUserId);
        Task AddMemberAsync(long teamId, long userId, long actorUserId);
        Task RemoveMemberAsync(long teamId, long userId, long actorUserId);
    }
}
