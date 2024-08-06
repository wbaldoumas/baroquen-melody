namespace BaroquenMelody.Library.Compositions.Rules.Enums;

/// <summary>
///     Represents the different configurable composition rules available for use.
/// </summary>
internal enum CompositionRule : byte
{
    /// <summary>
    ///     Avoid dissonant intervals.
    /// </summary>
    AvoidDissonance,

    /// <summary>
    ///     Avoid dissonant leaps.
    /// </summary>
    AvoidDissonantLeaps,

    /// <summary>
    ///     Handle ascending sevenths.
    /// </summary>
    HandleAscendingSeventh,

    /// <summary>
    ///    Avoid repetition of notes within the same voice.
    /// </summary>
    AvoidRepetition,

    /// <summary>
    ///     Avoid parallel fourths.
    /// </summary>
    AvoidParallelFourths,

    /// <summary>
    ///     Avoid parallel fifths.
    /// </summary>
    AvoidParallelFifths,

    /// <summary>
    ///     Avoid parallel octaves.
    /// </summary>
    AvoidParallelOctaves,

    /// <summary>
    ///     Avoid direct fourths.
    /// </summary>
    AvoidDirectFourths,

    /// <summary>
    ///     Avoid direct fifths.
    /// </summary>
    AvoidDirectFifths,

    /// <summary>
    ///     Avoid direct octaves.
    /// </summary>
    AvoidDirectOctaves,

    /// <summary>
    ///     Avoid over-doubling of voices on the same note.
    /// </summary>
    AvoidOverDoubling,

    /// <summary>
    ///     Follow standard chord progression.
    /// </summary>
    FollowStandardChordProgression
}
