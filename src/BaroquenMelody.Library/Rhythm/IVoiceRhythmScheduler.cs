using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Rhythm;

/// <summary>
///     Assigns per-voice rhythm roles over phrase-length blocks of the fugal composition body: which voice is
///     held (moving once per measure), which is florid (attracting more subdividing figures), and where the
///     held voice's note must be pinned during the body walk.
/// </summary>
internal interface IVoiceRhythmScheduler
{
    /// <summary>
    ///     Determine which instrument, if any, carries the held role for the given measure.
    /// </summary>
    /// <param name="measureIndex">The zero-based index of the measure within the composition body.</param>
    /// <param name="heldInstrument">The instrument holding through the measure, when one exists.</param>
    /// <returns>Whether a held instrument exists for the measure.</returns>
    bool TryGetHeldInstrument(int measureIndex, out Instrument heldInstrument);

    /// <summary>
    ///     Determine which instrument, if any, must have its previous note pinned at the given beat.
    /// </summary>
    /// <param name="measureIndex">The zero-based index of the measure within the composition body.</param>
    /// <param name="beatIndex">The zero-based index of the beat within its measure.</param>
    /// <param name="pinnedInstrument">The instrument whose previous note is pinned at the beat, when one exists.</param>
    /// <returns>Whether an instrument is pinned at the beat.</returns>
    bool TryGetPinnedInstrument(int measureIndex, int beatIndex, out Instrument pinnedInstrument);

    /// <summary>
    ///     Determine which instrument, if any, carries the florid role for the given measure.
    /// </summary>
    /// <param name="measureIndex">The zero-based index of the measure within the composition body.</param>
    /// <param name="floridInstrument">The instrument attracting subdividing figures in the measure, when one exists.</param>
    /// <returns>Whether a florid instrument exists for the measure.</returns>
    bool TryGetFloridInstrument(int measureIndex, out Instrument floridInstrument);
}
