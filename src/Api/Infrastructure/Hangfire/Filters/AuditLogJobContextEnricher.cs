using Hangfire.Server;

namespace Api.Infrastructure.Hangfire.Filters;

internal sealed class AuditLogJobContextEnricher : IServerFilter
{
    private const string AuditContextScopeKey = "AuditCtxScope";
    
    public void OnPerformed(PerformedContext context)
    {
        if (context.Items.TryGetValue(AuditContextScopeKey, out var obj) && obj is AuditContextScope auditContextScope)
        {
            auditContextScope.Dispose();
        }
    }

    public void OnPerforming(PerformingContext context)
    {
        var auditContext = AuditContext.BeginScope(
            ("JobId", context.BackgroundJob.Id),
            ("JobName", $"{context.BackgroundJob.Job.Type}.{context.BackgroundJob.Job.Method}"),
            ("JobArgs", string.Join(", ", context.BackgroundJob.Job.Args))
        );

        auditContext.AuditedBy = "Hangfire";

        context.Items[AuditContextScopeKey] = auditContext;
    }
}