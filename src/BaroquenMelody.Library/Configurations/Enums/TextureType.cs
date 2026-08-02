namespace BaroquenMelody.Library.Configurations.Enums;

/// <summary>
///     The accompaniment texture the fugal composition body renders under: how the voices below the melody
///     move once the body's roles begin. The frame - the fugal exposition and the composed ending - always
///     keeps its imitative fabric.
/// </summary>
public enum TextureType : byte
{
    /// <summary>
    ///     No accompaniment texture - the imitative default fabric, with rhythm roles rotating per phrase block.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Melody over sustained block harmonies: every accompanying voice moves plainly, tying repeated
    ///     harmonies and re-attacking where the harmony changes.
    /// </summary>
    Chordal,

    /// <summary>
    ///     Melody over a continuously treading bass: the lowest voice walks in even quarter motion while the
    ///     inner voices sustain.
    /// </summary>
    Walking,

    /// <summary>
    ///     Melody over broken-chord bass figuration: the lowest voice arpeggiates in even eighth patterns
    ///     while the inner voices sustain.
    /// </summary>
    BrokenChord
}
