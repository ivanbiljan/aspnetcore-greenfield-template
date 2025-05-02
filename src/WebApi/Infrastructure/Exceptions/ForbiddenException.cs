using System.Diagnostics.CodeAnalysis;

namespace WebApi.Infrastructure.Exceptions;

[SuppressMessage("Design", "CA1032:Implement standard exception constructors")]
internal sealed class ForbiddenException(string? message) : WebApiException(StatusCodes.Status403Forbidden, message)
{
}