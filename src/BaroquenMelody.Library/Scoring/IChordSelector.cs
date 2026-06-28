using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Scoring;

/// <summary>
///     Selects the next chord from a set of candidate chords which have already passed the hard
///     <see cref="Rules.ICompositionRule"/> pre-filters.
/// </summary>
internal interface IChordSelector
{
    /// <summary>
    ///     Selects the next chord from the given candidate chords.
    /// </summary>
    /// <param name="precedingChords">The chords which precede the proposed next chord.</param>
    /// <param name="candidateChords">The candidate chords to select from.</param>
    /// <returns>The selected chord, or <see langword="null"/> when there are no candidates.</returns>
    public BaroquenChord? SelectNextChord(IReadOnlyList<BaroquenChord> precedingChords, IEnumerable<BaroquenChord> candidateChords);
}
