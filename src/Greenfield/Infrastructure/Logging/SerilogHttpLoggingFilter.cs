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
///     <see cref="StartupExtensions.AddSerilogHttpLogging" />.
/// </param>
public sealed class SerilogHttpLoggingFilter(ILogger serilogLogger, IOptions<SerilogHttpLoggingOptions> options)
    : ExceptionFilterAttribute, IActionFilter, IEndpointFilter
{
    private const string RequestStartTimestampHttpItem = "RequestStartTimestamp";

    private const string DefaultMessageTemplate =
        "HTTP {Method} '{Path}' responded with {StatusCode} in {ElapsedMs:00}";

    private const string ExceptionMessageTemplate = "HTTP {Method} '{Path}' threw an exception";

    private readonly SerilogHttpLoggingOptions _loggingOptions = options.Value;
    private ILogger _contextualLogger = serilogLogger.ForContext<SerilogHttpLoggingFilter>();

    /// <inheritdoc />
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!_contextualLogger.IsEnabled(LogEventLevel.Information))
        {
            return;
        }
        
        context.HttpContext.Items[RequestStartTimestampHttpItem] = DateTime.UtcNow;

        _contextualLogger = PopulateRequestContext(context.HttpContext);

        if (_loggingOptions.LoggingFields.HasFlag(SerilogHttpLoggingFields.RequestBody))
        {
            foreach (var arg in context.ActionArguments)
            {
                _contextualLogger = _contextualLogger.ForContext(arg.Key, arg.Value, true);
            }
        }
    }

    /// <inheritdoc />
    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (!_contextualLogger.IsEnabled(LogEventLevel.Information))
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
        if (!_contextualLogger.IsEnabled(LogEventLevel.Information))
        {
            return await next(context);
        }
        
        var request = context.HttpContext.Request;
        _contextualLogger = PopulateRequestContext(context.HttpContext);

        if (_loggingOptions.LoggingFields.HasFlag(SerilogHttpLoggingFields.RequestBody))
        {
            _contextualLogger = _contextualLogger.ForContext("RequestArgs", context.Arguments, true);
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
            _contextualLogger.ForContext("ElapsedMs", stopwatch.ElapsedMilliseconds)
                .Error(ex, ExceptionMessageTemplate, request.Method, request.Path);

            throw;
        }
    }

    public override void OnException(ExceptionContext context)
    {
        var requestStartTimestamp = (DateTime) context.HttpContext.Items[RequestStartTimestampHttpItem];
        var elapsedMs = (DateTime.UtcNow - requestStartTimestamp).TotalMilliseconds;

        _contextualLogger.ForContext("ElapsedMs", elapsedMs)
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
        _contextualLogger = _contextualLogger.ForContext("Protocol", request.Protocol)
            .ForContext("Scheme", request.Scheme)
            .ForContext("Method", request.Method)
            .ForContext("Path", request.Path)
            .ForContext("User", context.User.Identity?.Name)
            .ForContext("RemoteIP", context.Connection.RemoteIpAddress);

        if (_loggingOptions.LoggingFields.HasFlag(SerilogHttpLoggingFields.RequestHeaders))
        {
            _contextualLogger = _contextualLogger.ForContext(
                "RequestHeaders",
                request.Headers.Where(h => _loggingOptions.RequestHeaders.Contains(h.Key))
            );
        }

        return _contextualLogger;
    }

    private void WriteResponse(HttpContext context, object? result, double elapsedMs)
    {
        if (_loggingOptions.LoggingFields.HasFlag(SerilogHttpLoggingFields.ResponseHeaders))
        {
            _contextualLogger = _contextualLogger.ForContext(
                "ResponseHeaders",
                context.Response.Headers.Where(h => _loggingOptions.ResponseHeaders.Contains(h.Key))
            );
        }

        if (_loggingOptions.LoggingFields.HasFlag(SerilogHttpLoggingFields.ResponseBody))
        {
            _contextualLogger = _contextualLogger.ForContext("ResponseBody", result, true);
        }

        var logLevel = context.Response.StatusCode >= 500 ? LogEventLevel.Error : LogEventLevel.Information;
        _contextualLogger.Write(
            logLevel,
            DefaultMessageTemplate,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMs
        );
    }
}