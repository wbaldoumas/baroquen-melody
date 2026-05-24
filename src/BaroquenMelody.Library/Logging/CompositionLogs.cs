using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Logging;

/// <summary>
///     Library-specific structured log messages emitted during composition.
/// </summary>
internal static partial class CompositionLogs
{
    [LoggerMessage(EventId = 100, Level = LogLevel.Information, Message = "Ornamentation {ornamentationType} applied to instrument {instrument}.")]
    public static partial void LogOrnamentationApplied(this ILogger logger, OrnamentationType ornamentationType, Instrument instrument);

    [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "Composed bridging chord {count} of {max}.")]
    public static partial void LogBridgingChordComposed(this ILogger logger, int count, int max);

    [LoggerMessage(EventId = 102, Level = LogLevel.Information, Message = "Composed chord {count} of {max} to tonic.")]
    public static partial void LogChordToTonicComposed(this ILogger logger, int count, int max);

    [LoggerMessage(EventId = 103, Level = LogLevel.Critical, Message = "Could not find starting note for instrument {instrument}.")]
    public static partial void LogCouldNotFindStartingNote(this ILogger logger, Instrument instrument);
}
