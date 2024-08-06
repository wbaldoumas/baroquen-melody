using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Library.Infrastructure.Logging;

/// <summary>
///     A home for log messages.
/// </summary>
internal static partial class Log
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Critical,
        Message = "No valid chord choices are currently available.")]
    public static partial void NoValidChordChoicesAvailable(this ILogger logger);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Composing...")]
    public static partial void Composing(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Composed main theme...")]
    public static partial void ComposedMainTheme(this ILogger logger);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Composed measure {MeasureNumber} of {TotalMeasures}...")]
    public static partial void ComposedMeasure(this ILogger logger, int measureNumber, int totalMeasures);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Composed composition continuation...")]
    public static partial void ComposedContinuation(this ILogger logger);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Information,
        Message = "Phrased composition...")]
    public static partial void PhrasedComposition(this ILogger logger);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Information,
        Message = "Composing composition ending...")]
    public static partial void ComposingEnding(this ILogger logger);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Composed composition ending...")]
    public static partial void ComposedEnding(this ILogger logger);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Warning,
        Message = "Could not find a suitable bridging chord after {MaxAttempts} attempts.")]
    public static partial void CouldNotFindSuitableBridgingChord(this ILogger logger, int maxAttempts);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Warning,
        Message = "Could not find a tonic chord after {MaxAttempts} attempts.")]
    public static partial void CouldNotFindTonicChord(this ILogger logger, int maxAttempts);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Warning,
        Message = "Failed to compose a fugue theme after {Attempt} attempts. Maximum attempts: {MaxAttempts}.")]
    public static partial void FailedToComposeFugalThemeAttempt(this ILogger logger, int attempt, int maxAttempts);

    [LoggerMessage(
        EventId = 11,
        Level = LogLevel.Warning,
        Message = "Failed to compose a fugue theme after {MaxAttempts} attempts.")]
    public static partial void FailedToComposeFugalTheme(this ILogger logger, int maxAttempts);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Critical,
        Message = "Could not find a starting note for voice {Voice}.")]
    public static partial void CouldNotFindStartingNoteForVoice(this ILogger logger, Voice voice);

    [LoggerMessage(
        EventId = 13,
        Level = LogLevel.Debug,
        Message = "Applied ornamentation {Ornamentation} to voice {Voice}.")]
    public static partial void AppliedOrnamentation(this ILogger logger, OrnamentationType ornamentation, Voice voice);

    [LoggerMessage(
        EventId = 14,
        Level = LogLevel.Debug,
        Message = "Repeated theme phrase.")]
    public static partial void RepeatedThemePhrase(this ILogger logger);

    [LoggerMessage(
        EventId = 15,
        Level = LogLevel.Debug,
        Message = "Repeated non-theme phrase.")]
    public static partial void RepeatedNonThemePhrase(this ILogger logger);

    [LoggerMessage(
        EventId = 16,
        Level = LogLevel.Debug,
        Message = "Composed bridging chord {ChordNumber} of {MaxBridgingChords}.")]
    public static partial void ComposedBridgingChord(this ILogger logger, int chordNumber, int maxBridgingChords);

    [LoggerMessage(
        EventId = 17,
        Level = LogLevel.Debug,
        Message = "Composed chord {ChordNumber} to tonic of {MaxChordsToTonic}.")]
    public static partial void ComposedChordToTonic(this ILogger logger, int chordNumber, int maxChordsToTonic);
}
