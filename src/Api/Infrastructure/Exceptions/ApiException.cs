using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Api.Infrastructure.Exceptions;

[SuppressMessage("Design", "CA1032:Implement standard exception constructors")]
internal class ApiException(int statusCode, string? message) : Exception(message)
{
    public ApiException(string message) : this((int) HttpStatusCode.UnprocessableContent, message)
    {
    }

    public int StatusCode { get; } = statusCode;
}