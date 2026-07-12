using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Strategies;

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
    ///     Gets the possible chords for the given preceding chords.
    /// </summary>
    /// <param name="precedingChords">The chords which precede the proposed next chord.</param>
    /// <returns>The possible chords for the given preceding chords.</returns>
    public IEnumerable<BaroquenChord> GetPossibleChords(IReadOnlyList<BaroquenChord> precedingChords);

    /// <summary>
    ///     Determines whether at least one rule-valid chord matches the given partially voiced
    ///     <paramref name="nextChord"/>. Unlike <see cref="GetPossibleChordsForPartiallyVoicedChords"/> this applies
    ///     no subsequent-chord look-ahead, making it a cheap existence check for steering pinned continuations.
    /// </summary>
    /// <param name="precedingChords">The chords which precede the proposed next chord.</param>
    /// <param name="nextChord">The proposed next chord.</param>
    /// <returns>Whether at least one rule-valid chord matches the partially voiced chord.</returns>
    public bool HasPossibleChordForPartiallyVoicedChord(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord);

    /// <summary>
    ///     Gets every rule-valid chord matching the given partially voiced <paramref name="nextChord"/>, without the
    ///     subsequent-chord look-ahead of <see cref="GetPossibleChordsForPartiallyVoicedChords"/>. Suits pinned
    ///     continuations whose next chord is already dictated (a fugal entry), where free-choice look-ahead only
    ///     starves the candidate set.
    /// </summary>
    /// <param name="precedingChords">The chords which precede the proposed next chord.</param>
    /// <param name="nextChord">The proposed next chord.</param>
    /// <returns>The rule-valid chords matching the partially voiced chord.</returns>
    public IReadOnlyList<BaroquenChord> GetRuleValidChordsForPartiallyVoicedChord(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord);
}
