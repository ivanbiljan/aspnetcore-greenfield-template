using Microsoft.Net.Http.Headers;

namespace Greenfield.Infrastructure.Logging;

/// <summary>
///     Specifies which HTTP request/response properties are logged through <see cref="SerilogHttpLoggingMiddleware" />.
/// </summary>
[Flags]
public enum SerilogHttpLoggingFields
{
    /// <summary>
    ///     <para>
    ///         Flag for logging the request headers.
    ///     </para>
    ///     <para>
    ///         Example:<br />
    ///         Content-Type: application/json<br />
    ///         Authorization: ****
    ///     </para>
    /// </summary>
    RequestHeaders = 1 << 0,

    /// <summary>
    ///     Flag for logging the request body. Properties annotated with Destructurama attributes are omitted.
    /// </summary>
    RequestBody = 1 << 1,

    /// <summary>
    ///     <para>
    ///         Flag for logging the response headers.
    ///     </para>
    ///     <para>
    ///         Example:<br />
    ///         Content-Type: application/json<br />
    ///         Authorization: ****
    ///     </para>
    /// </summary>
    ResponseHeaders = 1 << 2,

    /// <summary>
    ///     Flag for logging the response body. Properties annotated with Destructurama attributes are omitted.
    /// </summary>
    ResponseBody = 1 << 3,

    /// <summary>
    ///     A shorthand flag for logging the <see cref="RequestHeaders" /> and <see cref="RequestBody" />.
    /// </summary>
    Request = RequestHeaders | RequestBody,

    /// <summary>
    ///     A shorthand flag for logging the <see cref="ResponseHeaders" /> and
    ///     <see cref="ResponseBody" />.
    /// </summary>
    Response = ResponseHeaders | ResponseBody,

    /// <summary>
    ///     A shorthand flag for logging the entire request and response.
    /// </summary>
    All = Request | Response
}

/// <summary>
///     Defines options for the <see cref="SerilogHttpLoggingMiddleware" />.
/// </summary>
public sealed class SerilogHttpLoggingOptions
{
    /// <summary>
    ///     Gets or sets a bit vector that specifies which request/response properties are logged. Logs the path, request and
    ///     response headers by default.
    /// </summary>
    public SerilogHttpLoggingFields LoggingFields { get; set; } = SerilogHttpLoggingFields.RequestHeaders |
                                                                  SerilogHttpLoggingFields.ResponseHeaders;

    /// <summary>
    ///     Specifies the request headers that are allowed to be logged.
    /// </summary>
    public ISet<string> RequestHeaders { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        HeaderNames.Accept,
        HeaderNames.AcceptCharset,
        HeaderNames.AcceptEncoding,
        HeaderNames.AcceptLanguage,
        HeaderNames.Allow,
        HeaderNames.CacheControl,
        HeaderNames.Connection,
        HeaderNames.ContentEncoding,
        HeaderNames.ContentLength,
        HeaderNames.ContentType,
        HeaderNames.Date,
        HeaderNames.DNT,
        HeaderNames.Expect,
        HeaderNames.Host,
        HeaderNames.MaxForwards,
        HeaderNames.Range,
        HeaderNames.SecWebSocketExtensions,
        HeaderNames.SecWebSocketVersion,
        HeaderNames.TE,
        HeaderNames.Trailer,
        HeaderNames.TransferEncoding,
        HeaderNames.Upgrade,
        HeaderNames.UserAgent,
        HeaderNames.Warning,
        HeaderNames.XRequestedWith,
        HeaderNames.XUACompatible
    };

    /// <summary>
    ///     Specifies the response headers that are allowed to be logged.
    /// </summary>
    public ISet<string> ResponseHeaders { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        HeaderNames.AcceptRanges,
        HeaderNames.Age,
        HeaderNames.Allow,
        HeaderNames.AltSvc,
        HeaderNames.Connection,
        HeaderNames.ContentDisposition,
        HeaderNames.ContentLanguage,
        HeaderNames.ContentLength,
        HeaderNames.ContentLocation,
        HeaderNames.ContentRange,
        HeaderNames.ContentType,
        HeaderNames.Date,
        HeaderNames.Expires,
        HeaderNames.LastModified,
        HeaderNames.Location,
        HeaderNames.Server,
        HeaderNames.TransferEncoding,
        HeaderNames.Upgrade,
        HeaderNames.XPoweredBy
    };
}