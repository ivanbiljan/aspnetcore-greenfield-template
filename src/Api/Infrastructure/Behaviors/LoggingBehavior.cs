using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Immediate.Handlers.Shared;

namespace Api.Infrastructure.Behaviors;

[SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "ImmediateHandlers require behaviors to be public to be discoverable"
)]
public sealed class LoggingBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : Behavior<TRequest, TResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    /// <inheritdoc />
    public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var logContext = new Dictionary<string, object?>
        {
            ["@Request"] = request
        };

        using (_logger.BeginScope(logContext))
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is not null)
            {
                logContext["RequestMethod"] = httpContext.Request.Method;
                logContext["RequestPath"] = httpContext.Request.Path.ToString();
                logContext["User"] = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                logContext["RemoteIP"] = httpContext.Connection.RemoteIpAddress;
            }

            var requestType = typeof(TRequest);
            var handlerName = requestType.DeclaringType?.FullName ?? requestType.FullName!;

            try
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await Next(request, cancellationToken);
                stopwatch.Stop();

                logContext["@Response"] = response;

                _logger.LogInformation(
                    "{Handler} executed in {ElapsedTime} ms",
                    handlerName,
                    stopwatch.Elapsed.Milliseconds
                );

                return response;
            }
            catch (Exception ex)
            {
                if (ex is WebApiException)
                {
                    _logger.LogInformation("{Handler} returned an error: {Message}", handlerName, ex.Message);
                }
                else
                {
                    _logger.LogError(ex, "{Handler} returned an exception", handlerName);
                }

                throw;
            }
        }
    }
}