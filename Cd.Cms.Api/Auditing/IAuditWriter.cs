namespace Cd.Cms.Api.Auditing
{
    public interface IAuditWriter
    {
        Task WriteAsync(string entityType, long entityId, string action, long? actorUserId, string? ipAddress, string? oldValues, string? newValues, CancellationToken ct = default);
    }
}
