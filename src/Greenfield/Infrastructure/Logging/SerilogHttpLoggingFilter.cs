using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace Greenfield.Infrastructure.Logging;

/// <summary>
///     An action and endpoint filter that logs incoming HTTP requests and their respective responses; an alternative to
///     Serilog/Microsoft's HTTP logging middleware that works with Destructurama.Attributed.
/// </summary>
/// <param name="serilogLogger">The logger used to log the request.</param>
/// <param name="options">
///     The options that dictate which properties are logged. Configured through
///     <see cref="IServiceCollectionExtensions.AddSerilogHttpLogging" />.
/// </param>
public sealed class SerilogHttpLoggingFilter(ILogger serilogLogger, IOptions<SerilogHttpLoggingOptions> options)
    : ExceptionFilterAttribute, IActionFilter, IEndpointFilter
{
    private const string RequestStartTimestampHttpItem = "RequestStartTimestamp";

    private const string SuccessMessageTemplate =
        "HTTP {Method} '{Path}' responded with {StatusCode} in {ElapsedMs:00}";

    private const string ExceptionMessageTemplate = "HTTP {Method} '{Path}' threw an exception";

    private readonly SerilogHttpLoggingOptions _loggingOptions = options.Value;
    private ILogger _logger = serilogLogger.ForContext<SerilogHttpLoggingFilter>();

    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!_logger.IsEnabled(LogEventLevel.Information))
        {
            return;
        }
        
        context.HttpContext.Items[RequestStartTimestampHttpItem] = DateTime.UtcNow;

        _logger = PopulateRequestContext(context.HttpContext);

        if (_loggingOptions.LoggingFields.HasFlag(SerilogHttpLoggingFields.RequestBody))
        {
            foreach (var arg in context.ActionArguments)
            {
                _logger = _logger.ForContext(arg.Key, arg.Value, true);
            }
        }
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (!_logger.IsEnabled(LogEventLevel.Information))
        {
            return;
        }
        
        var requestStartTimestamp = (DateTime) context.HttpContext.Items[RequestStartTimestampHttpItem];
        var elapsedMs = (DateTime.UtcNow - requestStartTimestamp).TotalMilliseconds;

        WriteResponse(context.HttpContext, (context.Result as ObjectResult)?.Value, elapsedMs);
    }

    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!_logger.IsEnabled(LogEventLevel.Information))
        {
            return await next(context);
        }
        
        var request = context.HttpContext.Request;
        _logger = PopulateRequestContext(context.HttpContext);

        if (_loggingOptions.LoggingFields.HasFlag(SerilogHttpLoggingFields.RequestBody))
        {
            _logger = _logger.ForContext("RequestArgs", context.Arguments, true);
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            var result = await next(context);
            stopwatch.Stop();

            WriteResponse(context.HttpContext, result, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.ForContext("ElapsedMs", stopwatch.ElapsedMilliseconds)
                .Error(ex, ExceptionMessageTemplate, request.Method, request.Path);

            throw;
        }
    }

    public override void OnException(ExceptionContext context)
    {
        var requestStartTimestamp = (DateTime) context.HttpContext.Items[RequestStartTimestampHttpItem];
        var elapsedMs = (DateTime.UtcNow - requestStartTimestamp).TotalMilliseconds;

        _logger.ForContext("ElapsedMs", elapsedMs)
            .Error(
                context.Exception,
                ExceptionMessageTemplate,
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path
            );

        base.OnException(context);
    }

    private ILogger PopulateRequestContext(HttpContext context)
    {
        var request = context.Request;
        _logger = _logger.ForContext("Protocol", request.Protocol)
            .ForContext("Scheme", request.Scheme)
            .ForContext("Method", request.Method)
            .ForContext("Path", request.Path)
            .ForContext("User", context.User.Identity?.Name)
            .ForContext("RemoteIP", context.Connection.RemoteIpAddress);

        if (_loggingOptions.LoggingFields.HasFlag(SerilogHttpLoggingFields.RequestHeaders))
        {
            _logger = _logger.ForContext(
                "RequestHeaders",
                request.Headers.Where(h => _loggingOptions.RequestHeaders.Contains(h.Key))
            );
        }

        return _logger;
    }

    private void WriteResponse(HttpContext context, object? result, double elapsedMs)
    {
        if (_loggingOptions.LoggingFields.HasFlag(SerilogHttpLoggingFields.ResponseHeaders))
        {
            _logger = _logger.ForContext(
                "ResponseHeaders",
                context.Response.Headers.Where(h => _loggingOptions.ResponseHeaders.Contains(h.Key))
            );
        }

        if (_loggingOptions.LoggingFields.HasFlag(SerilogHttpLoggingFields.ResponseBody))
        {
            _logger = _logger.ForContext("ResponseBody", result, true);
        }

        var logLevel = context.Response.StatusCode >= 500 ? LogEventLevel.Error : LogEventLevel.Information;
        _logger.Write(
            logLevel,
            SuccessMessageTemplate,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs
        );
    }
}

public static class IServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the <see cref="SerilogHttpLoggingFilter" /> globally.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" />.</param>
    /// <param name="configure">The setup action used to configure the logging filter.</param>
    /// <returns>The modified <see cref="IServiceCollection" /> to allow chaining.</returns>
    public static IServiceCollection AddSerilogHttpLogging(
        this IServiceCollection services,
        Action<SerilogHttpLoggingOptions>? configure = null
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions();
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddControllersWithViews(
            options => { options.Filters.Add<SerilogHttpLoggingFilter>(); }
        );

        return services;
    }
}