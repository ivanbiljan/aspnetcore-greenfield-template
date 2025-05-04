using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Api.Database.Interceptors;

internal sealed class SlowQueryLoggingInterceptor(ILogger<SlowQueryLoggingInterceptor> logger) : DbCommandInterceptor
{
    private const int ThresholdInMilliseconds = 100;

    private readonly ILogger<SlowQueryLoggingInterceptor> _logger = logger;

    /// <inheritdoc />
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result
    )
    {
        if (eventData.Duration.Milliseconds >= ThresholdInMilliseconds)
        {
            _logger.LogWarning(
                "DB query took more than {ThresholdInMilliseconds}ms: {CommandText}",
                ThresholdInMilliseconds,
                command.CommandText
            );
        }

        return base.ReaderExecuted(command, eventData, result);
    }
}