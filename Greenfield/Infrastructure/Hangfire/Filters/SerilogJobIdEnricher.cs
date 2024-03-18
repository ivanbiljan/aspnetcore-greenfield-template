using Hangfire.Server;
using Serilog.Core;
using Serilog.Events;

namespace Greenfield.Infrastructure.Hangfire.Filters;

internal sealed class SerilogJobIdEnricher : IServerFilter, ILogEventEnricher
{
    private static readonly AsyncLocal<string?> HangfireJobId = new();

    /// <inheritdoc />
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var jobIdProperty = propertyFactory.CreateProperty("HangfireJobId", HangfireJobId.Value);
        logEvent.AddPropertyIfAbsent(jobIdProperty);
    }

    /// <inheritdoc />
    public void OnPerforming(PerformingContext context)
    {
        HangfireJobId.Value = context.BackgroundJob.Id;
    }

    /// <inheritdoc />
    public void OnPerformed(PerformedContext context)
    {
        HangfireJobId.Value = null;
    }
}