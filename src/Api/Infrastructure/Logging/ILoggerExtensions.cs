namespace Microsoft.Extensions.Logging;

internal static class ILoggerExtensions
{
    public static IDisposable? BeginScope<TLogger>(
        this ILogger<TLogger> logger,
        params (string key, object? value)[] ambientContext
    )
    {
        return logger.BeginScope(ambientContext.ToDictionary(p => p.key, p => p.value));
    }
}