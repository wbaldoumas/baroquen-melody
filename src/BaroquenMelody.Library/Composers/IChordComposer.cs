using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Composers;

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

    /// <summary>
    ///     Composes the next chord in the sequence of <paramref name="precedingChords"/>, preferring chords
    ///     that repeat <paramref name="pinnedNote"/> in its voice and falling back to the free choice when no
    ///     candidate honors the pin.
    /// </summary>
    /// <param name="precedingChords">The preceding chords used to generate the next chord.</param>
    /// <param name="pinnedNote">The note the held voice should repeat when the rule set allows it.</param>
    /// <returns>A <see cref="BaroquenChord"/> to continue with from the preceding chords.</returns>
    BaroquenChord Compose(IReadOnlyList<BaroquenChord> precedingChords, BaroquenNote pinnedNote);
}
