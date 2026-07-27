using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     Configures the ground bass form: a short bass pattern announced alone, then repeated under a freshly
///     composed upper texture, closing with a cadence onto the tonic - the passacaglia's shape. When enabled
///     it replaces the fugal form for the whole composition; the fugue remains the default.
/// </summary>
/// <param name="Enabled"> Whether the composition takes the ground bass form instead of the fugal form. </param>
[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record GroundBassConfiguration(bool Enabled)
{
    public static GroundBassConfiguration Default { get; } = new(Enabled: false);
}
