using Destructurama;
using Microsoft.Net.Http.Headers;
using Serilog;
using Serilog.AspNetCore;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Refit.Destructurers;

namespace Greenfield.Infrastructure.Logging;

public static class StartupExtensions
{
    private static readonly HashSet<string> RequestHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        HeaderNames.Accept,
        HeaderNames.AcceptCharset,
        HeaderNames.AcceptEncoding,
        HeaderNames.AcceptLanguage,
        HeaderNames.Allow,
        HeaderNames.CacheControl,
        HeaderNames.Connection,
        HeaderNames.ContentEncoding,
        HeaderNames.ContentLength,
        HeaderNames.ContentType,
        HeaderNames.Date,
        HeaderNames.DNT,
        HeaderNames.Expect,
        HeaderNames.Host,
        HeaderNames.MaxForwards,
        HeaderNames.Range,
        HeaderNames.SecWebSocketExtensions,
        HeaderNames.SecWebSocketVersion,
        HeaderNames.TE,
        HeaderNames.Trailer,
        HeaderNames.TransferEncoding,
        HeaderNames.Upgrade,
        HeaderNames.UserAgent,
        HeaderNames.Warning,
        HeaderNames.XRequestedWith,
        HeaderNames.XUACompatible,
        "X-Amzn-Trace-Id",
        "X-Forwarded-For"
    };

    private static readonly HashSet<string> ResponseHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        HeaderNames.AcceptRanges,
        HeaderNames.Age,
        HeaderNames.Allow,
        HeaderNames.AltSvc,
        HeaderNames.Connection,
        HeaderNames.ContentDisposition,
        HeaderNames.ContentLanguage,
        HeaderNames.ContentLength,
        HeaderNames.ContentLocation,
        HeaderNames.ContentRange,
        HeaderNames.ContentType,
        HeaderNames.Date,
        HeaderNames.Expires,
        HeaderNames.LastModified,
        HeaderNames.Location,
        HeaderNames.Server,
        HeaderNames.TransferEncoding,
        HeaderNames.Upgrade,
        HeaderNames.XPoweredBy
    };
    
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
    ///     Configures Serilog's request logging middleware. Enriches the diagnostic context with protcol, scheme and request headers.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" />.</param>
    /// <returns>The modified <see cref="IServiceCollection" /> to allow chaining.</returns>
    public static IServiceCollection AddSerilogHttpLogging(this IServiceCollection services)
    {
        services.Configure<RequestLoggingOptions>(
            options =>
            {
                options.EnrichDiagnosticContext = (context, httpContext) =>
                {
                    context.Set("RequestProtocol", httpContext.Request.Protocol);
                    context.Set("RequestScheme", httpContext.Request.Scheme);
                    context.Set(
                        "RequestHeaders",
                        httpContext.Request.Headers.Where(h => RequestHeaders.Contains(h.Key))
                    );

                    context.Set("User", httpContext.User.Identity?.Name);
                    context.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.MapToIPv4());
                };
            }
        );
        
        return services;
    }
}