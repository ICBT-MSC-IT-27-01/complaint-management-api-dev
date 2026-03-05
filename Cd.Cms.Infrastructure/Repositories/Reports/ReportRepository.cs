using Cd.Cms.Application.Contracts.Repositories;
using Cd.Cms.Application.DTOs.Reports;
using Cd.Cms.Infrastructure.Contracts;
using Cd.Cms.Shared;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Cd.Cms.Infrastructure.Repositories.Reports
{
    public class ReportRepository : IReportRepository
    {
        private readonly IDbFactory _db;
        public ReportRepository(IDbFactory db) => _db = db;

        public async Task<DashboardDto> GetDashboardAsync(long actorUserId, string role, DashboardRequest request)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ReportSpNames.GetDashboard, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@ActorUserId", actorUserId);
            cmd.Parameters.AddWithValue("@Role",        role);
            cmd.Parameters.AddWithValue("@Period",      request.Period);
            cmd.Parameters.AddWithValue("@From",        (object?)request.From ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@To",          (object?)request.To ?? DBNull.Value);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            var dto = new DashboardDto();
            if (await r.ReadAsync())
            {
                dto.TotalComplaints  = DataReader.GetInt(r, "TotalComplaints");
                dto.OpenComplaints   = DataReader.GetInt(r, "OpenComplaints");
                dto.ResolvedToday    = DataReader.GetInt(r, "ResolvedToday");
                dto.SlaBreached      = DataReader.GetInt(r, "SlaBreached");
                dto.SlaAtRisk        = DataReader.GetInt(r, "SlaAtRisk");
                dto.MyOpenComplaints = DataReader.GetInt(r, "MyOpenComplaints");
                dto.AvgResolutionHours = DataReader.GetDecimal(r, "AvgResolutionHours") > 0 ? (double)DataReader.GetDecimal(r, "AvgResolutionHours") : 0d;
            }
            if (await r.NextResultAsync())
                while (await r.ReadAsync())
                    dto.ByStatus.Add(new StatusCountDto { Status = DataReader.GetString(r, "Status"), Count = DataReader.GetInt(r, "Count") });
            if (await r.NextResultAsync())
                while (await r.ReadAsync())
                    dto.ByPriority.Add(new PriorityCountDto { Priority = DataReader.GetString(r, "Priority"), Count = DataReader.GetInt(r, "Count") });

            dto.SlaCompliancePercent = dto.TotalComplaints == 0 ? 0 : Math.Round(((dto.TotalComplaints - dto.SlaBreached) * 100.0) / dto.TotalComplaints, 2);
            return dto;
        }

        public async Task<ComplaintSummaryDto> GetComplaintSummaryAsync(ReportFilterRequest req)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(ReportSpNames.GetComplaintSummary, conn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@From",         (object?)req.From         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@To",           (object?)req.To           ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@AgentUserId",  (object?)req.AgentUserId  ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CategoryId",   (object?)req.CategoryId   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Department",   (object?)req.Department   ?? DBNull.Value);
            await conn.OpenAsync();
            using var r = await cmd.ExecuteReaderAsync();
            var dto = new ComplaintSummaryDto();
            if (await r.ReadAsync())
            {
                dto.TotalComplaints  = DataReader.GetInt(r, "TotalComplaints");
                dto.NewCount         = DataReader.GetInt(r, "NewCount");
                dto.InProgressCount  = DataReader.GetInt(r, "InProgressCount");
                dto.ResolvedCount    = DataReader.GetInt(r, "ResolvedCount");
                dto.ClosedCount      = DataReader.GetInt(r, "ClosedCount");
                dto.EscalatedCount   = DataReader.GetInt(r, "EscalatedCount");
                dto.SlaBreachedCount = DataReader.GetInt(r, "SlaBreachedCount");
            }
            if (await r.NextResultAsync())
                while (await r.ReadAsync())
                    dto.AgentStats.Add(new AgentPerformanceDto {
                        UserId = DataReader.GetLong(r,"UserId"), AgentName = DataReader.GetString(r,"AgentName"),
                        Assigned = DataReader.GetInt(r,"Assigned"), Resolved = DataReader.GetInt(r,"Resolved"),
                        AvgResolutionHours = (double)DataReader.GetDecimal(r,"AvgResolutionHours") });
            if (await r.NextResultAsync())
                while (await r.ReadAsync())
                    dto.CategoryStats.Add(new CategoryCountDto { Category = DataReader.GetString(r,"Category"), Count = DataReader.GetInt(r,"Count") });
            return dto;
        }
    }
}
