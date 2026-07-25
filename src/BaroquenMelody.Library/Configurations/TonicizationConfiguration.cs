using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     Configures tonicization: at strong-beat harmonic changes, a minor triad whose root sits a perfect
///     fifth above its successor's root may raise its third by a semitone, turning it into a true dominant
///     of the chord it approaches - the raised leading tone the diatonic scale cannot express.
/// </summary>
/// <param name="Enabled"> Whether tonicization is applied at all. </param>
/// <param name="Probability"> The probability (0-100) that an eligible tonicization site is actually raised. </param>
[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record TonicizationConfiguration(bool Enabled, int Probability)
{
    public static TonicizationConfiguration Default { get; } = new(Enabled: true, Probability: 100);
}
