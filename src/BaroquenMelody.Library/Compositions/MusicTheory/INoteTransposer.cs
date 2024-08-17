using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.MusicTheory;

/// <summary>
///     Represents a transposer that can transpose notes to different instruments.
/// </summary>
internal interface INoteTransposer
{
    /// <summary>
    ///     Transposes the given notes to the specified instrument.
    /// </summary>
    /// <param name="notesToTranspose">The notes to transpose.</param>
    /// <param name="sourceInstrument">The source instrument of the notes.</param>
    /// <param name="targetInstrument">The target instrument to transpose the notes to.</param>
    /// <returns>The transposed notes.</returns>
    IEnumerable<BaroquenNote> TransposeToInstrument(IEnumerable<BaroquenNote> notesToTranspose, Instrument sourceInstrument, Instrument targetInstrument);
}
