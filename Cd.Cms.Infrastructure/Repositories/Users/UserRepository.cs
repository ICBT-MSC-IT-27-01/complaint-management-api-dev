using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.DTOs.Users;
using Cd.Cms.Infrastructure.Contracts;
using Cd.Cms.Shared;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Cd.Cms.Infrastructure.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbFactory _db;
        public UserRepository(IDbFactory db) => _db = db;

        public async Task<UserDto?> GetByIdAsync(long id)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(UserSpNames.GetById, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            return await r.ReadAsync() ? MapUser(r) : null;
        }

        public async Task<UserDto?> GetByEmailOrUsernameAsync(string emailOrUsername)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(UserSpNames.GetByEmailOrUsername, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@EmailOrUsername", emailOrUsername);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            return await r.ReadAsync() ? MapUser(r) : null;
        }

        public async Task<AuthUserDto?> GetAuthUserByEmailOrUsernameAsync(string emailOrUsername)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(UserSpNames.GetByEmailOrUsername, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@EmailOrUsername", emailOrUsername);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            return await r.ReadAsync() ? MapAuthUser(r) : null;
        }

        public async Task<PagedResult<UserDto>> SearchAsync(UserSearchRequest request)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(UserSpNames.Search, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Keyword",  (object?)request.Keyword  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Role",     (object?)request.Role     ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Department",(object?)request.Department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", (object?)request.IsActive ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Page",     request.Page);
            cmd.Parameters.AddWithValue("@PageSize", request.PageSize);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            var items = new List<UserDto>();
            while (await r.ReadAsync()) items.Add(MapUser(r));
            long total = 0;
            if (await r.NextResultAsync() && await r.ReadAsync()) total = r.GetInt64(0);
            return new PagedResult<UserDto> { Page = request.Page, PageSize = request.PageSize, TotalCount = total, Items = items.ToArray() };
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest req, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(UserSpNames.Create, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Name",         req.Name);
            cmd.Parameters.AddWithValue("@Email",        req.Email);
            cmd.Parameters.AddWithValue("@Username",     req.Username);
            cmd.Parameters.AddWithValue("@PhoneNumber",  (object?)req.PhoneNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PasswordHash", req.Password);
            cmd.Parameters.AddWithValue("@Role",         req.Role);
            cmd.Parameters.AddWithValue("@Department",   (object?)req.Department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReportingManagerId", (object?)req.ReportingManagerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActorUserId",  actorUserId);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync()) return MapUser(r);
            throw new InvalidOperationException("User creation failed.");
        }

        public async Task UpdateAsync(long id, UpdateUserRequest req, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(UserSpNames.Update, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id",          id);
            cmd.Parameters.AddWithValue("@Name",        req.Name);
            cmd.Parameters.AddWithValue("@Email",       req.Email);
            cmd.Parameters.AddWithValue("@Username",    req.Username);
            cmd.Parameters.AddWithValue("@PhoneNumber", (object?)req.PhoneNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Role",        req.Role);
            cmd.Parameters.AddWithValue("@Department",   (object?)req.Department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ReportingManagerId", (object?)req.ReportingManagerId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(long id, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(UserSpNames.Delete, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id",          id);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ChangePasswordAsync(long id, string newPasswordHash, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(UserSpNames.ChangePassword, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@Id",              id);
            cmd.Parameters.AddWithValue("@NewPasswordHash", newPasswordHash);
            cmd.Parameters.AddWithValue("@ActorUserId",     actorUserId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<UserDto>> GetAgentsAsync()
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(UserSpNames.GetAgents, conn) { CommandType = CommandType.StoredProcedure };
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            var list = new List<UserDto>();
            while (await r.ReadAsync()) list.Add(MapUser(r));
            return list;
        }

        private static UserDto MapUser(SqlDataReader r) => new()
        {
            Id               = DataReader.GetLong(r, "Id"),
            Name             = DataReader.GetString(r, "Name"),
            Email            = DataReader.GetString(r, "Email"),
            Username         = DataReader.GetString(r, "Username"),
            PhoneNumber      = DataReader.GetString(r, "PhoneNumber"),
            Role             = DataReader.GetString(r, "Role"),
            IsActive         = DataReader.GetBool(r, "IsActive"),
            IsLocked         = DataReader.GetBool(r, "IsLocked"),
            Department       = DataReader.GetString(r, "Department"),
            ReportingManagerId = DataReader.GetNullableLong(r, "ReportingManagerId"),
            ReportingManagerName = DataReader.GetString(r, "ReportingManagerName"),
            CreatedDateTime  = DataReader.GetDate(r, "CreatedDateTime"),
            LastLoginDateTime = DataReader.GetNullableDate(r, "LastLoginDateTime"),
        };

        private static AuthUserDto MapAuthUser(SqlDataReader r) => new()
        {
            Id           = DataReader.GetLong(r, "Id"),
            Name         = DataReader.GetString(r, "Name"),
            Email        = DataReader.GetString(r, "Email"),
            Username     = DataReader.GetString(r, "Username"),
            Role         = DataReader.GetString(r, "Role"),
            IsActive     = DataReader.GetBool(r, "IsActive"),
            IsLocked     = DataReader.GetBool(r, "IsLocked"),
            PasswordHash = DataReader.GetString(r, "PasswordHash"),
        };
    }
}
