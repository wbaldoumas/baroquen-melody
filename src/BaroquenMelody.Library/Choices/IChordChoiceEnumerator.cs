using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Choices;

/// <summary>
///     Enumerates the chord-choice candidates reachable from a given chord, in the canonical repository order,
///     paired with the chords they materialize to.
/// </summary>
internal interface IChordChoiceEnumerator
{
    /// <summary>
    ///     Enumerates the candidate chord choices reachable from <paramref name="currentChord"/> together with the
    ///     chords they produce, in the same order the Cartesian chord-choice repository would enumerate them.
    /// </summary>
    /// <param name="currentChord">The chord the candidates move from.</param>
    /// <returns>The candidate chord choices and their materialized chords.</returns>
    IEnumerable<(ChordChoice ChordChoice, BaroquenChord Chord)> EnumerateCandidates(BaroquenChord currentChord);
}
