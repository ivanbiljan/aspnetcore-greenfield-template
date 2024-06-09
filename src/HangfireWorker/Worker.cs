using Greenfield.Infrastructure.Hangfire;

namespace HangfireWorker;

public class Worker(IServiceProvider serviceProvider) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RecurringJobProvider.ScheduleRecurringJobsForCurrentAssembly(serviceProvider);
        
        return Task.CompletedTask;
    }
}