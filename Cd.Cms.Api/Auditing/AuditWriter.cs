using Cd.Cms.Infrastructure.Contracts;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Cd.Cms.Api.Auditing
{
    public sealed class AuditWriter : IAuditWriter
    {
        private readonly IDbFactory _db;
        public AuditWriter(IDbFactory db) => _db = db;

        public async Task WriteAsync(string entityType, long entityId, string action, long? actorUserId, string? ipAddress, string? oldValues, string? newValues, CancellationToken ct = default)
        {
            using var conn = (SqlConnection)_db.CreateConnection();
            using var cmd = new SqlCommand(@"
INSERT INTO AuditLogs (EntityType, EntityId, Action, OldValues, NewValues, PerformedByUserId, IPAddress)
VALUES (@EntityType, @EntityId, @Action, @OldValues, @NewValues, @PerformedByUserId, @IPAddress);", conn)
            { CommandType = CommandType.Text };

            cmd.Parameters.AddWithValue("@EntityType", entityType);
            cmd.Parameters.AddWithValue("@EntityId", entityId);
            cmd.Parameters.AddWithValue("@Action", action);
            cmd.Parameters.AddWithValue("@OldValues", (object?)oldValues ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@NewValues", (object?)newValues ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PerformedByUserId", (object?)actorUserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IPAddress", (object?)ipAddress ?? DBNull.Value);

            await conn.OpenAsync(ct);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
