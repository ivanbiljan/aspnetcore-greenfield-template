using System.Globalization;
using Api.Features.Authentication;
using Immediate.Handlers.Shared;

namespace Api.Infrastructure.Behaviors;

internal sealed class AuditLogEnrichingBehavior<TRequest, TResponse>(
    IUserContext userContext
) : Behavior<TRequest, TResponse>
{
    private readonly IUserContext _userContext = userContext;

    public override async ValueTask<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var auditLogActor = _userContext.IsAuthenticated
            ? $"User ID {_userContext.GetId().ToString(CultureInfo.InvariantCulture)}"
            : _userContext.UserAgent;

        using var auditContext = new AuditContext();
        auditContext.AuditedBy = auditLogActor;
        auditContext.SetProperty("UserIP", _userContext.IpAddress);

        return await Next(request, cancellationToken);
    }
}