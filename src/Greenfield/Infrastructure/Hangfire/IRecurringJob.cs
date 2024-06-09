using Hangfire.Server;

namespace Greenfield.Infrastructure.Hangfire;

/// <summary>
///     Describes a Hangfire job that is scheduled to run on a regular basis as dictated by its cron expression.
/// </summary>
public interface IRecurringJob
{
    /// <summary>
    ///     Gets a cron expression that defines how often the job is executed.
    /// </summary>
    string Cron { get; }
    
    /// <summary>
    ///     Gets a unique identifier for this job. Assume that identifiers are case-sensitive.
    /// </summary>
    string JobId { get; }
    
    /// <summary>
    ///     The method invoked by Hangfire to execute the job.
    /// </summary>
    /// <param name="context">
    ///     The <see cref="PerformContext" /> that is auto-populated by Hangfire when execution starts. Can
    ///     be used to log information to the job's output stream so that it is visible in the dashboard.
    /// </param>
    /// <returns>A task for this action.</returns>
    Task ExecuteAsync(PerformContext context);
}