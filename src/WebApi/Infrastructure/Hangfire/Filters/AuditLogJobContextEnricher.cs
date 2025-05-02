using Hangfire.Server;

namespace WebApi.Infrastructure.Hangfire.Filters;

internal sealed class AuditLogJobContextEnricher : IServerFilter
{
    public void OnPerforming(PerformingContext context)
    {
        var auditContext = new AuditContext(
            ("JobId", context.BackgroundJob.Id),
            ("JobName", $"{context.BackgroundJob.Job.Type}.{context.BackgroundJob.Job.Method}"),
            ("JobArgs", string.Join(", ", context.BackgroundJob.Job.Args))
        )
        {
            AuditedBy = "Hangfire"
        };

        context.Items["AuditContext"] = auditContext;
    }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Items.TryGetValue("AuditContext", out var obj) && obj is AuditContext auditContext)
        {
            auditContext.Dispose();
        }
    }
}