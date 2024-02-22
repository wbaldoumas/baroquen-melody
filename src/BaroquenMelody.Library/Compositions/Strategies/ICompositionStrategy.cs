using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Strategies;

/// <summary>
///     Represents a strategy for composing a melody. When given a <see cref="ChordContext" />, it will return the
///     next <see cref="ChordChoice" /> to be used to get to the next chord.
///
///     Can also be used to invalidate a <see cref="ChordChoice" /> if it is no longer valid for the given context.
/// </summary>
internal interface ICompositionStrategy
{
    /// <summary>
    ///    Get the next <see cref="ChordChoice" /> to be used for the given <see cref="ChordContext" /> we are currently in.
    /// </summary>
    /// <param name="chordContext">The <see cref="ChordContext" /> we are currently in.</param>
    /// <returns>The next <see cref="ChordChoice" /> to be used for the given <see cref="ChordContext" /> we are currently in.</returns>
    ChordChoice GetNextChordChoice(ChordContext chordContext);

    /// <summary>
    ///    Invalidate the given <see cref="ChordChoice" /> for the given <see cref="ChordContext" />.
    /// </summary>
    /// <param name="chordContext"> The <see cref="ChordContext" /> we are currently in.</param>
    /// <param name="chordChoice"> The <see cref="ChordChoice" /> to invalidate.</param>
    void InvalidateChordChoice(ChordContext chordContext, ChordChoice chordChoice);

    /// <summary>
    ///     Selects an initial chord to start the composition.
    /// </summary>
    /// <returns> The initial chord to start the composition. </returns>
    public ContextualizedChord GetInitialChord();
}
