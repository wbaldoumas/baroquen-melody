namespace BaroquenMelody.Library.MusicTheory.Enums;

/// <summary>
///     Represents the type of cadence formed by a penultimate chord resolving to a final chord.
/// </summary>
internal enum CadenceType : byte
{
    /// <summary>
    ///     No recognized cadence.
    /// </summary>
    None = 0,

    /// <summary>
    ///     A perfect authentic cadence: V to I with both chords in root position and the tonic in the soprano
    ///     of the final chord.
    /// </summary>
    PerfectAuthentic,

    /// <summary>
    ///     An imperfect authentic cadence: V to I not meeting the perfect authentic criteria (an inverted chord,
    ///     or a soprano ending off the tonic).
    /// </summary>
    ImperfectAuthentic,

    /// <summary>
    ///     A half cadence: a phrase ending on V.
    /// </summary>
    Half,

    /// <summary>
    ///     A Phrygian half cadence: IV in first inversion resolving to V with the bass descending by semitone.
    /// </summary>
    PhrygianHalf,

    /// <summary>
    ///     A deceptive cadence: V resolving to VI instead of the expected I.
    /// </summary>
    Deceptive,

    /// <summary>
    ///     A plagal cadence: IV to I.
    /// </summary>
    Plagal
}
