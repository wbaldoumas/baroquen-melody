using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     Configures how the harmonic rhythm of the composition body varies: phrase-interior measures hold each
///     harmony across two beats while the measure before each phrase seam returns to per-beat harmonic motion,
///     accelerating into the cadence.
/// </summary>
/// <param name="Enabled"> Whether the harmonic rhythm varies at all. When <see langword="false"/>, every beat carries a freshly composed chord. </param>
[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record HarmonicRhythmConfiguration(bool Enabled)
{
    public static HarmonicRhythmConfiguration Default { get; } = new(Enabled: true);
}
