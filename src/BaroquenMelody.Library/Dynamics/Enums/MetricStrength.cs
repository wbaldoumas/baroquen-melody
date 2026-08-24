namespace BaroquenMelody.Library.Dynamics.Enums;

/// <summary>
///     The metric strength of a beat within its (hyper)measure, used to shape expressive dynamics.
/// </summary>
internal enum MetricStrength : byte
{
    /// <summary>
    ///     A metrically weak beat.
    /// </summary>
    Weak,

    /// <summary>
    ///     A secondary (medium) accent, such as the mid-point of a four-beat measure.
    /// </summary>
    Medium,

    /// <summary>
    ///     A metrically strong beat, such as the downbeat of a measure.
    /// </summary>
    Strong
}
