using BaroquenMelody.Library.Forms.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     Configures the ground bass form: a short bass pattern announced alone, then repeated under a freshly
///     composed upper texture, closing with a cadence onto the tonic - the passacaglia's shape. When enabled
///     it replaces the fugal form for the whole composition; the fugue remains the default.
/// </summary>
/// <param name="Enabled"> Whether the composition takes the ground bass form instead of the fugal form. </param>
/// <param name="Pattern">
///     The specific bank pattern to state, or <see langword="null"/> to let the composer draw among the
///     patterns that fit the lowest voice's range. A configured pattern the range cannot host yields no
///     plan, falling back to the fugal form the way an empty bank does.
/// </param>
[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record GroundBassConfiguration(bool Enabled, GroundBass? Pattern = null)
{
    public static GroundBassConfiguration Default { get; } = new(Enabled: false);
}
