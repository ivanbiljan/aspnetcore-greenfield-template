using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Api.Infrastructure.Exceptions;

[SuppressMessage("Design", "CA1032:Implement standard exception constructors")]
internal class WebApiException(int statusCode, string? message) : Exception(message)
{
    public WebApiException(string message) : this((int) HttpStatusCode.UnprocessableContent, message)
    {
    }

    public int StatusCode { get; } = statusCode;
}