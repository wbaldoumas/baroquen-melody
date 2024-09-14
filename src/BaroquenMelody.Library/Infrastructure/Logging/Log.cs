﻿using BaroquenMelody.Library.Compositions.Enums;
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
        Message = "{message}: {exceptionType}: {exceptionMessage}\n Stack trace: {stackTrace}")]
    public static partial void LogException(this ILogger logger, string message, string exceptionType, string exceptionMessage, string? stackTrace);

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Critical,
        Message = "No valid chord choices are currently available.")]
    public static partial void NoValidChordChoicesAvailable(this ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Warning,
        Message = "Could not find a suitable bridging chord after {MaxAttempts} attempts.")]
    public static partial void CouldNotFindSuitableBridgingChord(this ILogger logger, int maxAttempts);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "Could not find a tonic chord after {MaxAttempts} attempts.")]
    public static partial void CouldNotFindTonicChord(this ILogger logger, int maxAttempts);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Warning,
        Message = "Failed to compose a fugue theme after {Attempt} attempts. Maximum attempts: {MaxAttempts}.")]
    public static partial void FailedToComposeFugalThemeAttempt(this ILogger logger, int attempt, int maxAttempts);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Warning,
        Message = "Failed to compose a fugue theme after {MaxAttempts} attempts.")]
    public static partial void FailedToComposeFugalTheme(this ILogger logger, int maxAttempts);

    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Critical,
        Message = "Could not find a starting note for instrument {Instrument}.")]
    public static partial void CouldNotFindStartingNoteForInstrument(this ILogger logger, Instrument instrument);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Debug,
        Message = "Applied ornamentation {Ornamentation} to instrument {Instrument}.")]
    public static partial void AppliedOrnamentation(this ILogger logger, OrnamentationType ornamentation, Instrument instrument);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Debug,
        Message = "Repeated theme phrase.")]
    public static partial void RepeatedThemePhrase(this ILogger logger);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Debug,
        Message = "Repeated non-theme phrase.")]
    public static partial void RepeatedNonThemePhrase(this ILogger logger);

    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Debug,
        Message = "Composed bridging chord {ChordNumber} of {MaxBridgingChords}.")]
    public static partial void ComposedBridgingChord(this ILogger logger, int chordNumber, int maxBridgingChords);

    [LoggerMessage(
        EventId = 12,
        Level = LogLevel.Debug,
        Message = "Composed chord {ChordNumber} to tonic of {MaxChordsToTonic}.")]
    public static partial void ComposedChordToTonic(this ILogger logger, int chordNumber, int maxChordsToTonic);
}
