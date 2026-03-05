using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.DTOs.Complaints;
using Cd.Cms.Infrastructure.Contracts;
using Cd.Cms.Shared;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Cd.Cms.Infrastructure.Repositories.Complaints
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly IDbFactory _db;
        public ComplaintRepository(IDbFactory db) => _db = db;

        public async Task<ComplaintDto?> GetByIdAsync(long id)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.GetById, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ComplaintId", id);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            return await r.ReadAsync() ? MapComplaint(r) : null;
        }

        public async Task<PagedResult<ComplaintListItemDto>> SearchAsync(ComplaintSearchRequest req)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.Search, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@StatusId",        (object?)req.StatusId        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId",      (object?)req.CategoryId      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ChannelId",       (object?)req.ChannelId       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Department",      (object?)req.Department      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Priority",        (object?)req.Priority        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AssignedToUserId",(object?)req.AssignedToUserId?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Q",               (object?)req.Q               ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@From",            (object?)req.From            ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@To",              (object?)req.To              ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Page",            req.Page);
            cmd.Parameters.AddWithValue("@PageSize",        req.PageSize);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            var items = new List<ComplaintListItemDto>();
            while (await r.ReadAsync()) items.Add(MapListItem(r));
            long total = 0;
            if (await r.NextResultAsync() && await r.ReadAsync()) total = r.GetInt64(0);
            return new PagedResult<ComplaintListItemDto> { Page = req.Page, PageSize = req.PageSize, TotalCount = total, Items = items.ToArray() };
        }

        public async Task<ComplaintDto> CreateAsync(CreateComplaintRequest req, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.Create, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ClientId",            (object?)req.ClientId            ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientName",          (object?)req.ClientName          ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientEmail",         (object?)req.ClientEmail         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ClientMobile",        (object?)req.ClientMobile        ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ComplaintChannelId",  req.ComplaintChannelId);
            cmd.Parameters.AddWithValue("@ComplaintCategoryId", req.ComplaintCategoryId);
            cmd.Parameters.AddWithValue("@SubCategoryId",       (object?)req.SubCategoryId       ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Subject",             req.Subject);
            cmd.Parameters.AddWithValue("@Description",         req.Description);
            cmd.Parameters.AddWithValue("@Priority",            req.Priority);
            cmd.Parameters.AddWithValue("@ActorUserId",         actorUserId);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync()) return MapComplaint(r);
            throw new InvalidOperationException("Complaint creation failed.");
        }

        public async Task UpdateAsync(long id, CreateComplaintRequest req, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.Update, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ComplaintId",         id);
            cmd.Parameters.AddWithValue("@Subject",             req.Subject);
            cmd.Parameters.AddWithValue("@Description",         req.Description);
            cmd.Parameters.AddWithValue("@Priority",            req.Priority);
            cmd.Parameters.AddWithValue("@ComplaintChannelId",  req.ComplaintChannelId);
            cmd.Parameters.AddWithValue("@ComplaintCategoryId", req.ComplaintCategoryId);
            cmd.Parameters.AddWithValue("@ActorUserId",         actorUserId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task AssignAsync(long id, AssignComplaintRequest req, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.Assign, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ComplaintId",      id);
            cmd.Parameters.AddWithValue("@AssignedToUserId", req.AssignedToUserId);
            cmd.Parameters.AddWithValue("@DueDate",          (object?)req.DueDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Note",             (object?)req.Note    ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActorUserId",      actorUserId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateStatusAsync(long id, UpdateComplaintStatusRequest req, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.UpdateStatus, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ComplaintId", id);
            cmd.Parameters.AddWithValue("@StatusId",    req.StatusId);
            cmd.Parameters.AddWithValue("@Note",        (object?)req.Note ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task EscalateAsync(long id, EscalateComplaintRequest req, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.Escalate, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ComplaintId",      id);
            cmd.Parameters.AddWithValue("@Reason",           req.Reason);
            cmd.Parameters.AddWithValue("@EscalatedToUserId",(object?)req.EscalatedToUserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@EscalationType",   req.EscalationType);
            cmd.Parameters.AddWithValue("@ActorUserId",      actorUserId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ResolveAsync(long id, ResolveComplaintRequest req, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.Resolve, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ComplaintId",       id);
            cmd.Parameters.AddWithValue("@ResolutionSummary", req.ResolutionSummary);
            cmd.Parameters.AddWithValue("@RootCause",         (object?)req.RootCause   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FixApplied",        (object?)req.FixApplied  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ActorUserId",       actorUserId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task CloseAsync(long id, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.Close, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ComplaintId", id);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(long id, long actorUserId)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.Delete, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ComplaintId", id);
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<ComplaintHistoryDto>> GetHistoryAsync(long id)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ComplaintSpNames.GetHistory, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ComplaintId", id);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            var list = new List<ComplaintHistoryDto>();
            while (await r.ReadAsync())
                list.Add(new ComplaintHistoryDto
                {
                    Id              = DataReader.GetLong(r, "Id"),
                    Action          = DataReader.GetString(r, "Action"),
                    OldStatus       = DataReader.GetString(r, "OldStatus"),
                    NewStatus       = DataReader.GetString(r, "NewStatus"),
                    Note            = DataReader.GetString(r, "Note"),
                    PerformedByName = DataReader.GetString(r, "PerformedByName"),
                    CreatedDateTime = DataReader.GetDate(r, "CreatedDateTime"),
                });
            return list;
        }

        private static ComplaintDto MapComplaint(SqlDataReader r) => new()
        {
            Id                = DataReader.GetLong(r, "Id"),
            ComplaintNumber   = DataReader.GetString(r, "ComplaintNumber"),
            Subject           = DataReader.GetString(r, "Subject"),
            Description       = DataReader.GetString(r, "Description"),
            Priority          = DataReader.GetString(r, "Priority"),
            Status            = DataReader.GetString(r, "Status"),
            ComplaintStatusId = DataReader.GetLong(r, "ComplaintStatusId"),
            ComplaintChannelId= DataReader.GetLong(r, "ComplaintChannelId"),
            Channel           = DataReader.GetString(r, "Channel"),
            ComplaintCategoryId=DataReader.GetLong(r, "ComplaintCategoryId"),
            Category          = DataReader.GetString(r, "Category"),
            ClientId          = DataReader.GetNullableLong(r, "ClientId"),
            ClientName        = DataReader.GetString(r, "ClientName"),
            ClientEmail       = DataReader.GetString(r, "ClientEmail"),
            ClientMobile      = DataReader.GetString(r, "ClientMobile"),
            AssignedToUserId  = DataReader.GetNullableLong(r, "AssignedToUserId"),
            AssignedToName    = DataReader.GetString(r, "AssignedToName"),
            AssignedDate      = DataReader.GetNullableDate(r, "AssignedDate"),
            DueDate           = DataReader.GetNullableDate(r, "DueDate"),
            SlaStatus         = DataReader.GetString(r, "SlaStatus"),
            IsSlaBreached     = DataReader.GetBool(r, "IsSlaBreached"),
            IsResolved        = DataReader.GetBool(r, "IsResolved"),
            ResolvedDate      = DataReader.GetNullableDate(r, "ResolvedDate"),
            ResolutionNotes   = DataReader.GetString(r, "ResolutionNotes"),
            IsClosed          = DataReader.GetBool(r, "IsClosed"),
            ClosedDate        = DataReader.GetNullableDate(r, "ClosedDate"),
            CreatedDateTime   = DataReader.GetDate(r, "CreatedDateTime"),
            CreatedBy         = DataReader.GetLong(r, "CreatedBy"),
            CreatedByName     = DataReader.GetString(r, "CreatedByName"),
            UpdatedDateTime   = DataReader.GetNullableDate(r, "UpdatedDateTime"),
            IsActive          = DataReader.GetBool(r, "IsActive"),
        };

        private static ComplaintListItemDto MapListItem(SqlDataReader r) => new()
        {
            Id                = DataReader.GetLong(r, "Id"),
            ComplaintNumber   = DataReader.GetString(r, "ComplaintNumber"),
            Subject           = DataReader.GetString(r, "Subject"),
            Priority          = DataReader.GetString(r, "Priority"),
            Status            = DataReader.GetString(r, "Status"),
            ComplaintStatusId = DataReader.GetLong(r, "ComplaintStatusId"),
            Category          = DataReader.GetString(r, "Category"),
            ClientName        = DataReader.GetString(r, "ClientName"),
            AssignedToName    = DataReader.GetString(r, "AssignedToName"),
            SlaStatus         = DataReader.GetString(r, "SlaStatus"),
            DueDate           = DataReader.GetNullableDate(r, "DueDate"),
            CreatedDateTime   = DataReader.GetDate(r, "CreatedDateTime"),
            IsActive          = DataReader.GetBool(r, "IsActive"),
        };
    }
}
