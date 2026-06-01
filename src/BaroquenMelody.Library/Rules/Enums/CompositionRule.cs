namespace BaroquenMelody.Library.Rules.Enums;

/// <summary>
///     Represents the different configurable composition rules available for use.
/// </summary>
public enum CompositionRule : byte
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
    ///    Avoid repetition of notes within the same instrument.
    /// </summary>
    AvoidRepeatedNotes,

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
    ///     Avoid over-doubling of instruments on the same note.
    /// </summary>
    AvoidOverDoubling,

    /// <summary>
    ///     Follow standard chord progression.
    /// </summary>
    FollowStandardChordProgression,

    /// <summary>
    ///     Avoid repeated chords.
    /// </summary>
    AvoidRepeatedChords,

    /// <summary>
    ///     Avoid voice crossing, where a lower-register voice rises above an adjacent higher-register voice.
    /// </summary>
    AvoidVoiceCrossing,

    /// <summary>
    ///     Avoid voice overlap, where a voice moves beyond the previous position of an adjacent voice.
    /// </summary>
    AvoidVoiceOverlap,

    /// <summary>
    ///     Enforce that adjacent upper voices are spaced no more than an octave apart.
    /// </summary>
    EnforceVoiceSpacing
}
