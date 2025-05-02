using WebApi.Infrastructure.Hangfire;

namespace Worker;

internal sealed class Worker(IServiceProvider serviceProvider) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RecurringJobProvider.ScheduleRecurringJobsForCurrentAssembly(serviceProvider);
        
        return Task.CompletedTask;
    }
}