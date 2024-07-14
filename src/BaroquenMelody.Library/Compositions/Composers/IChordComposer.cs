using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Composers;

/// <summary>
///     Represents a composer which can compose the next chord in a sequence of preceding chords.
/// </summary>
internal interface IChordComposer
{
    /// <summary>
    ///     Composes the next chord in the sequence of <paramref name="precedingChords"/>.
    /// </summary>
    /// <param name="precedingChords">The preceding chords used to generate the next chord.</param>
    /// <returns>A <see cref="BaroquenChord"/> to continue with from the preceding chords.</returns>
    BaroquenChord Compose(IReadOnlyList<BaroquenChord> precedingChords);
}
