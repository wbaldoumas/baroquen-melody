using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.MusicTheory;

/// <summary>
///     Applies tonicization to a composition: at strong-beat harmonic changes, a minor triad whose root
///     sits a perfect fifth above its successor's root may raise its third by a semitone, becoming a true
///     dominant of the chord it approaches.
/// </summary>
internal interface ITonicizationApplicator
{
    /// <summary>
    ///     Raise the thirds of eligible dominant-functioning chords in the given composition.
    /// </summary>
    /// <param name="composition">The composition to tonicize.</param>
    void ApplyTonicization(Composition composition);
}
