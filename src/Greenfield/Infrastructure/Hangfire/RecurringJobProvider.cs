using System.Reflection;
using Hangfire;
using Hangfire.Common;

namespace Greenfield.Infrastructure.Hangfire;

public static class RecurringJobProvider
{
    /// <summary>
    ///     Scans the entry assembly for types derived from <see cref="IRecurringJob" /> and creates a
    ///     <see cref="RecurringJob" /> for each. Cron expressions are evaluated against UTC.
    /// </summary>
    public static void ScheduleRecurringJobsForCurrentAssembly(IServiceProvider serviceProvider)
    {
        ScheduleRecurringJobsForAssembly(Assembly.GetEntryAssembly()!, serviceProvider);
    }
    
    /// <summary>
    ///     Scans the provided assembly for types derived from <see cref="IRecurringJob" /> and creates a
    ///     <see cref="RecurringJob" /> for each. Cron expressions are evaluated against UTC.
    /// </summary>
    public static void ScheduleRecurringJobsForAssembly(Assembly assembly, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        
        var recurringJobTypesFromEntryAssembly = assembly
            .GetTypes()
            .Where(x => !x.IsAbstract)
            .Where(x => !x.IsInterface)
            .Where(x => x.IsClass)
            .Where(x => typeof(IRecurringJob).IsAssignableFrom(x))
            .ToList();
        
        var recurringJobManager = serviceProvider.GetRequiredService<IRecurringJobManager>();
        
        using var serviceScope = serviceProvider.CreateScope();
        foreach (var jobHandler in recurringJobTypesFromEntryAssembly)
        {
            var job = (IRecurringJob) ActivatorUtilities.CreateInstance(
                serviceScope.ServiceProvider,
                jobHandler
            );
            
            // Provide null for the PerformContext and Hangfire fills it with the correct context when executing it
            recurringJobManager.AddOrUpdate(
                job.JobId,
                Job.FromExpression(() => job.ExecuteAsync(null!)),
                job.Cron
            );
        }
    }
}