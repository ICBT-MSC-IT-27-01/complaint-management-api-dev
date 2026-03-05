using System.Security.Claims;

namespace Cd.Cms.Api.Auditing
{
    public sealed class AuditLoggingMiddleware
    {
        private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase) { "POST", "PUT", "PATCH", "DELETE" };
        private readonly RequestDelegate _next;

        public AuditLoggingMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, IAuditWriter auditWriter)
        {
            await _next(context);

            if (!MutatingMethods.Contains(context.Request.Method)) return;
            if (context.Response.StatusCode >= 500) return;

            try
            {
                var path = context.Request.Path.Value ?? string.Empty;
                if (!path.StartsWith("/api/v1", StringComparison.OrdinalIgnoreCase)) return;

                var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var entityType = segments.Length >= 3 ? segments[2] : "Unknown";
                long entityId = 0;
                if (segments.Length >= 4 && long.TryParse(segments[3], out var parsedId))
                    entityId = parsedId;

                var uidText = context.User.FindFirst("uid")?.Value;
                long? actorId = long.TryParse(uidText, out var uid) ? uid : null;
                var action = context.Request.Method;
                var ip = context.Connection.RemoteIpAddress?.ToString();

                var payload = $"Path={path};Status={context.Response.StatusCode}";
                await auditWriter.WriteAsync(entityType, entityId, action, actorId, ip, null, payload, context.RequestAborted);
            }
            catch
            {
                // Swallow audit failures to avoid impacting business APIs.
            }
        }
    }
}
