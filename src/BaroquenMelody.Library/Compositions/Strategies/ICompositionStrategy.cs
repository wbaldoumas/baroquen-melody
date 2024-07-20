using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;

namespace BaroquenMelody.Library.Compositions.Strategies;

/// <summary>
///     Represents a strategy for composing a melody.
/// </summary>
internal interface ICompositionStrategy
{
    /// <summary>
    ///     Generates the initial chord for a composition.
    /// </summary>
    /// <returns>The initial chord for a composition.</returns>
    public BaroquenChord GenerateInitialChord();

    /// <summary>
    ///    Gets the possible chord choices for the given preceding chords.
    /// </summary>
    /// <param name="precedingChords">The chords which precede the proposed next chord.</param>
    /// <returns>The possible chord choices for the given preceding chords.</returns>
    public IReadOnlyList<ChordChoice> GetPossibleChordChoices(IReadOnlyList<BaroquenChord> precedingChords);

    /// <summary>
    ///     Gets the possible chords for the given partially voiced <paramref name="nextChord"/>.
    /// </summary>
    /// <param name="precedingChords">The chords which precede the proposed next chord.</param>
    /// <param name="nextChord">The proposed next chord.</param>
    /// <returns>All the possible next chords for the given partially voiced chord.</returns>
    public IReadOnlyList<BaroquenChord> GetPossibleChordsForPartiallyVoicedChords(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord);

    /// <summary>
    ///     Gets the possible chords for the given <paramref name="chordNumber"/>.
    /// </summary>
    /// <param name="precedingChords">The chords which precede the proposed next chord.</param>
    /// <param name="chordNumber">The chord number for which to get possible chords.</param>
    /// <returns>All the possible next chords for the given chord number.</returns>
    public IReadOnlyList<BaroquenChord> GetPossibleChordsForChordNumber(IReadOnlyList<BaroquenChord> precedingChords, ChordNumber chordNumber);
}
