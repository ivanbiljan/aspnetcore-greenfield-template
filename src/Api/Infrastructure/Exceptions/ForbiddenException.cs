using System.Diagnostics.CodeAnalysis;

namespace Api.Infrastructure.Exceptions;

[SuppressMessage("Design", "CA1032:Implement standard exception constructors")]
internal sealed class ForbiddenException(string? message) : ApiException(StatusCodes.Status403Forbidden, message)
{
}