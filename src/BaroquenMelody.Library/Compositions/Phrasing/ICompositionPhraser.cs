using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Phrasing;

/// <summary>
/// Represents a phraser for a composition.
/// </summary>
internal interface ICompositionPhraser
{
    /// <summary>
    /// Attempts to repeat a phrase in the composition.
    /// </summary>
    /// <param name="measures">The measures that make up the composition.</param>
    void AttemptPhraseRepetition(List<Measure> measures);
}
