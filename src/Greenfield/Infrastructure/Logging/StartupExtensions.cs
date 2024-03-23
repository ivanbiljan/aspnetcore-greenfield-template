using Destructurama;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;

namespace Greenfield.Infrastructure.Logging;

public static class StartupExtensions
{
    /// <summary>
    ///     Sets Serilog as the default logging provider and configures its enrichers and sinks.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to configure.</param>
    /// <param name="projectName">The name of the project all logs will be enriched with.</param>
    /// <returns>The modified <paramref name="services" /> to allow chaining.</returns>
    public static IServiceCollection ConfigureSerilog(this IServiceCollection services, string projectName)
    {
        return services.AddSerilog(
            (_, loggerConfiguration) =>
            {
                loggerConfiguration.MinimumLevel.Information();
                loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
                loggerConfiguration.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);
                loggerConfiguration.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information);

                loggerConfiguration.Enrich.FromLogContext();
                loggerConfiguration.Enrich.WithMachineName();
                loggerConfiguration.Enrich.WithEnvironmentName();
                loggerConfiguration.Enrich.WithProperty("Project", projectName);
                loggerConfiguration.Enrich.WithExceptionDetails(
                    new DestructuringOptionsBuilder().WithDestructurers(
                        new[]
                        {
                            new ApiExceptionDestructurer()
                        }
                    )
                );

                loggerConfiguration.Destructure.UsingAttributes();

                loggerConfiguration.WriteTo.Console();
            }
        );
    }

    /// <summary>
    ///     Adds Serilog's HTTP request logging middleware to the pipeline. Enriches requests with the user's identity and
    ///     RemoteIpAddress. Request and response bodies are not logged.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder" />.</param>
    /// <returns>The modified <paramref name="app" />.</returns>
    public static IApplicationBuilder UseSerilogHttpLogging(this IApplicationBuilder app)
    {
        return app.UseSerilogRequestLogging(
            options =>
            {
                options.EnrichDiagnosticContext = static (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("User", httpContext.User.Identity?.Name);
                    diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress);
                };
            }
        );
    }
}