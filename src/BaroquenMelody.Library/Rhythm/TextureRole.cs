namespace BaroquenMelody.Library.Rhythm;

/// <summary>
///     A voice's part in an active accompaniment texture: one melody on top, one figuration carrier at the
///     bottom, and sustained pads between them.
/// </summary>
internal enum TextureRole : byte
{
    /// <summary>
    ///     The highest voice: keeps free decoration, recorded florid so the subdividing tier tilts toward it.
    /// </summary>
    Melody,

    /// <summary>
    ///     The lowest voice: carries the texture's figure family at near-certainty, everything else silenced.
    /// </summary>
    Figuration,

    /// <summary>
    ///     An inner voice: ornament-silenced and tied wherever its pairs repeat - the sustained inner harmony.
    /// </summary>
    Pad
}
