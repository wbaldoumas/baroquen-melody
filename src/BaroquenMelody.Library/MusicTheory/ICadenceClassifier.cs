using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;

namespace BaroquenMelody.Library.MusicTheory;

/// <summary>
///     Classifies the cadence formed by a pair of chords within the context of a given musical key.
/// </summary>
internal interface ICadenceClassifier
{
    /// <summary>
    ///     Classify the cadence formed by the given penultimate chord resolving to the given final chord.
    /// </summary>
    /// <param name="penultimateChord">The chord approaching the cadence.</param>
    /// <param name="finalChord">The chord of arrival.</param>
    /// <returns>The <see cref="CadenceType"/> the two chords form.</returns>
    CadenceType ClassifyCadence(BaroquenChord penultimateChord, BaroquenChord finalChord);
}
