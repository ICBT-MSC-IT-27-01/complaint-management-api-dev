using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.DTOs.Teams;
using Cd.Cms.Infrastructure.Contracts;
using Cd.Cms.Shared;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Cd.Cms.Infrastructure.Repositories.Teams
{
    public sealed class TeamRepository : ITeamRepository
    {
        private readonly IDbFactory _db;
        public TeamRepository(IDbFactory db) => _db = db;

        public async Task<TeamDto?> GetByIdAsync(long id)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(TeamSpNames.GetById, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);

            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();

            TeamDto? team = null;
            if (await r.ReadAsync()) team = MapTeam(r);
            if (team == null) return null;

            if (await r.NextResultAsync())
            {
                while (await r.ReadAsync()) team.Members.Add(MapMember(r));
            }

            team.MemberCount = team.Members.Count;
            return team;
        }

        public async Task<PagedResult<TeamDto>> SearchAsync(TeamSearchRequest req)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(TeamSpNames.Search, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Q", (object?)req.Q ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", (object?)req.IsActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Page", req.Page);
            cmd.Parameters.AddWithValue("@PageSize", req.PageSize);

            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();

            var items = new List<TeamDto>();
            while (await r.ReadAsync()) items.Add(MapTeam(r));

            long total = 0;
            if (await r.NextResultAsync() && await r.ReadAsync()) total = r.GetInt64(0);

            return new PagedResult<TeamDto>
            {
                Page = req.Page,
                PageSize = req.PageSize,
                TotalCount = total,
                Items = items.ToArray()
            };
        }

        public async Task<TeamDto> CreateAsync(CreateTeamRequest req, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(TeamSpNames.Create, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@TeamName", req.TeamName);
            cmd.Parameters.AddWithValue("@LeadUserId", (object?)req.LeadUserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);

            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();

            if (await r.ReadAsync()) return MapTeam(r);
            throw new InvalidOperationException("Team creation failed.");
        }

        public async Task UpdateAsync(long id, UpdateTeamRequest req, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(TeamSpNames.Update, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@TeamName", req.TeamName);
            cmd.Parameters.AddWithValue("@LeadUserId", (object?)req.LeadUserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", req.IsActive);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(long id, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(TeamSpNames.Delete, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AddMemberAsync(long teamId, long userId, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(TeamSpNames.AddMember, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@TeamId", teamId);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveMemberAsync(long teamId, long userId, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(TeamSpNames.RemoveMember, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@TeamId", teamId);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private static TeamDto MapTeam(SqlDataReader r) => new()
        {
            Id = DataReader.GetLong(r, "Id"),
            TeamCode = DataReader.GetString(r, "TeamCode"),
            TeamName = DataReader.GetString(r, "TeamName"),
            LeadUserId = DataReader.GetNullableLong(r, "LeadUserId"),
            LeadName = DataReader.GetString(r, "LeadName"),
            IsActive = DataReader.GetBool(r, "IsActive"),
            MemberCount = DataReader.GetInt(r, "MemberCount"),
            CreatedDateTime = DataReader.GetDate(r, "CreatedDateTime")
        };

        private static TeamMemberDto MapMember(SqlDataReader r) => new()
        {
            UserId = DataReader.GetLong(r, "UserId"),
            Name = DataReader.GetString(r, "Name"),
            Email = DataReader.GetString(r, "Email"),
            Role = DataReader.GetString(r, "Role")
        };
    }
}
