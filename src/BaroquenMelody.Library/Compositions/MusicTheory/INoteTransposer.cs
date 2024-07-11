using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.MusicTheory;

/// <summary>
///     Represents a transposer that can transpose notes to different voices.
/// </summary>
internal interface INoteTransposer
{
    /// <summary>
    ///     Transposes the given notes to the specified voice.
    /// </summary>
    /// <param name="notesToTranspose">The notes to transpose.</param>
    /// <param name="currentVoice">The current voice of the notes.</param>
    /// <param name="targetVoice">The target voice to transpose the notes to.</param>
    /// <returns>The transposed notes.</returns>
    IEnumerable<BaroquenNote> TransposeToVoice(IEnumerable<BaroquenNote> notesToTranspose, Voice currentVoice, Voice targetVoice);
}
