using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Contexts;

/// <summary>
///    Generates chord contexts.
/// </summary>
internal interface IChordContextGenerator
{
    /// <summary>
    ///     Generates a chord context from the previous and current chords.
    /// </summary>
    /// <param name="previousChord"> The previous chord. </param>
    /// <param name="currentChord"> The current chord. </param>
    /// <returns> The generated chord context. </returns>
    ChordContext GenerateChordContext(ContextualizedChord previousChord, ContextualizedChord currentChord);
}
