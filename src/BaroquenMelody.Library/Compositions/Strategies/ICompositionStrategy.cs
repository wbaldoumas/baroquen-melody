using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;

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
}
