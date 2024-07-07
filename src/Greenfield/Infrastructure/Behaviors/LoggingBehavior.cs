using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Greenfield.Infrastructure.Behaviors;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Implicitly invoked by MediatR")]
internal sealed class LoggingBehavior<TRequest, TResponse>(
    IHttpContextAccessor httpContextAccessor,
    UserManager<IdentityUser> userManager,
    ILogger logger
) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private ILogger _logger = logger.ForContext<LoggingBehavior<TRequest, TResponse>>();
    
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        _logger = logger.ForContext("Request", request, true);
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            _logger = logger.ForContext("RequestMethod", httpContext.Request.Method)
                .ForContext("RequestPath", httpContext.Request.Path.ToString())
                .ForContext("User", _userManager.GetUserId(httpContext.User))
                .ForContext("RemoteIP", httpContext.Connection.RemoteIpAddress);
        }
        
        var requestType = typeof(TRequest);
        var handlerName = requestType.DeclaringType?.FullName ?? requestType.FullName!;
        
        try
        {
            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();
            
            _logger.ForContext("Response", response, true)
                .Information(
                    "{Handler} executed in {ElapsedTime:000} ms",
                    handlerName,
                    stopwatch.Elapsed.TotalMilliseconds
                );
            
            return response;
        }
        catch (Exception ex) when (ex is not ApplicationException and not ValidationException)
        {
            _logger.Error(ex, "{Handler} returned an exception", handlerName);
            
            throw;
        }
    }
}