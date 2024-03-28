using System.Diagnostics;
using MediatR;

namespace Greenfield.Infrastructure.Behaviors;

internal sealed class LoggingBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    ILogger<LoggingBehavior<TRequest, TResponse>> logger
) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var additionalLogProperties = new Dictionary<string, object?>
        {
            ["Request"] = request
        };

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            additionalLogProperties["RequestMethod"] = httpContext.Request.Method;
            additionalLogProperties["RequestPath"] = httpContext.Request.Path.ToString();

            additionalLogProperties["User"] = httpContext.User.Identity?.Name;
            additionalLogProperties["RemoteIP"] = httpContext.Connection.RemoteIpAddress;
        }

        var requestType = typeof(TRequest);
        var handlerName = requestType.DeclaringType?.FullName ?? requestType.FullName!;

        using (logger.BeginScope(additionalLogProperties))
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var response = await next();
                stopwatch.Stop();

                logger.LogInformation(
                    "{Handler} executed in {ElapsedTime:000} ms",
                    handlerName,
                    stopwatch.Elapsed.TotalMilliseconds
                );

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Handler} returned an exception", handlerName);
                throw;
            }
        }
    }
}