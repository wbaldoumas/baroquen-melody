using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     Configures prepared suspensions: at strong-beat harmonic changes, a voice's chord tone may be tied
///     across the barline, sounding as an accented dissonance against the new chord before resolving down by
///     step - the preparation-and-tie the re-articulated appoggiatura lacks.
/// </summary>
/// <param name="Enabled"> Whether suspensions are applied at all. </param>
/// <param name="Probability"> The probability (0-100) that an eligible suspension site is actually suspended. </param>
[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record SuspensionConfiguration(bool Enabled, int Probability)
{
    public static SuspensionConfiguration Default { get; } = new(Enabled: true, Probability: 75);
}
