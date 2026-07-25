using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Ornamentation;

/// <summary>
///     Applies prepared suspensions at strong-beat harmonic changes: a voice's chord tone is tied across the
///     barline, sounding as an accented dissonance against the new chord before resolving down by step.
/// </summary>
internal interface ISuspensionApplicator
{
    /// <summary>
    ///     Apply suspensions to eligible strong-beat harmonic changes throughout the composition.
    /// </summary>
    /// <param name="composition">The composition to apply suspensions to.</param>
    void ApplySuspensions(Composition composition);
}
