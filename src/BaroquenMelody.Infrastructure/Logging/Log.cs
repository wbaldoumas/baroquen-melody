using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Infrastructure.Logging;

/// <summary>
///     A home for log messages.
/// </summary>
public static partial class Log
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Trace, Message = "{message}")]
    public static partial void LogTraceMessage(this ILogger logger, string message);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "{message}")]
    public static partial void LogInfoMessage(this ILogger logger, string message);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "{message}")]
    public static partial void LogDebugMessage(this ILogger logger, string message);

    [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "{message}")]
    public static partial void LogWarningMessage(this ILogger logger, string message);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "{message}")]
    public static partial void LogErrorMessage(this ILogger logger, string message);

    [LoggerMessage(EventId = 5, Level = LogLevel.Critical, Message = "{message}")]
    public static partial void LogCriticalMessage(this ILogger logger, string message);

    [LoggerMessage(EventId = 6, Level = LogLevel.Critical, Message = "{message}: {exceptionType}: {exceptionMessage}\n Stack trace: {stackTrace}")]
    public static partial void LogException(this ILogger logger, string message, string exceptionType, string exceptionMessage, string? stackTrace);
}
