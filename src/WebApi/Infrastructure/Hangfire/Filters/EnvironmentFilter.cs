using System.Text.Json;
using Hangfire.States;

namespace WebApi.Infrastructure.Hangfire.Filters;

internal sealed class EnvironmentFilter(IHostEnvironment hostEnvironment) : IElectStateFilter
{
    private readonly IHostEnvironment _hostEnvironment = hostEnvironment;

    /// <inheritdoc />
    public void OnStateElection(ElectStateContext context)
    {
        var allowedEnvironments = JsonSerializer.Deserialize<List<string>>(
            context.Connection.GetAllEntriesFromHash($"recurring-job:{context.BackgroundJob.Id}")
                .GetValueOrDefault("AllowedEnvironments", """["Development", "Staging", "Production"]""")
        )!;

        if (allowedEnvironments.Contains(_hostEnvironment.EnvironmentName))
        {
            return;
        }

        context.CandidateState = new DeletedState();
    }
}