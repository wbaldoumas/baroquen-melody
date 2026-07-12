namespace BaroquenMelody.Library.MusicTheory.Enums;

/// <summary>
///     Represents the inversion of a chord, as determined by which chord tone sounds in the bass voice.
/// </summary>
internal enum ChordInversion : byte
{
    /// <summary>
    ///     An unknown inversion: the chord could not be identified, or its bass note is not a chord tone.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     Root position: the root of the chord sounds in the bass voice.
    /// </summary>
    RootPosition,

    /// <summary>
    ///     First inversion: the third of the chord sounds in the bass voice.
    /// </summary>
    FirstInversion,

    /// <summary>
    ///     Second inversion: the fifth of the chord sounds in the bass voice.
    /// </summary>
    SecondInversion
}
