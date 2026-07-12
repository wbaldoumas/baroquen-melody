using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.MusicTheory;

/// <summary>
///     Places a fugal entry (a statement of the subject or answer) within a target instrument's range.
/// </summary>
internal interface IFugalEntryPlacer
{
    /// <summary>
    ///     Places the given fugal entry within the target instrument's configured range by transposing it in
    ///     whole octaves, which preserves the entry's pitch classes (and therefore its identity as the subject or
    ///     answer). The octave leaving the fewest principal notes unviable (out of range, beyond the voice spacing
    ///     rule's feasible notes, or - for the first note - unreachable from <paramref name="precedingNote"/>) is
    ///     chosen; ties are broken toward the fewest notes outside the range, then the most centered placement, and
    ///     then the lower placement. When no fully viable placement exists, the least-unviable octave is still
    ///     chosen and the notes are left at their true pitch.
    /// </summary>
    /// <param name="entry">The fugal entry (subject or answer) to place, voiced at its original pitch.</param>
    /// <param name="targetInstrument">The instrument whose range the entry is placed within.</param>
    /// <param name="precedingNote">
    ///     The note the target instrument sounds immediately before the entry begins, when known; the first placed
    ///     note must be reachable from it within the chord search's per-beat motion limit.
    /// </param>
    /// <returns>The entry transposed into the target instrument's register.</returns>
    IReadOnlyList<BaroquenNote> Place(IReadOnlyList<BaroquenNote> entry, Instrument targetInstrument, BaroquenNote? precedingNote = null);
}
