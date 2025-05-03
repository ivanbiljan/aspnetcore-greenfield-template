using System.Globalization;
using Immediate.Handlers.Shared;
using WebApi.Infrastructure.Web;

namespace WebApi.Infrastructure.Behaviors;

internal sealed class AuditLogEnrichingBehavior<TRequest, TResponse>(
    ICurrentUserService currentUserService
) : Behavior<TRequest, TResponse>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;

    public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var auditLogActor = _currentUserService.IsAuthenticated
            ? $"User ID {_currentUserService.GetId().ToString(CultureInfo.InvariantCulture)}"
            : _currentUserService.UserAgent;

        using var auditContext = new AuditContext();
        auditContext.AuditedBy = auditLogActor;
        auditContext.SetProperty("UserIP", _currentUserService.IpAddress);

        return await Next(request, cancellationToken);
    }
}