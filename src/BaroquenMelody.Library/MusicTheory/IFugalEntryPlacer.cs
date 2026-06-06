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
    ///     answer). The octave that leaves the fewest notes outside the range is chosen; ties are broken toward the
    ///     most centered placement, and then toward the lower placement. When the entry is taller than the range,
    ///     the least-spilling octave is still chosen and the spilling notes are left at their true pitch.
    /// </summary>
    /// <param name="entry">The fugal entry (subject or answer) to place, voiced at its original pitch.</param>
    /// <param name="targetInstrument">The instrument whose range the entry is placed within.</param>
    /// <returns>The entry transposed into the target instrument's register.</returns>
    IReadOnlyList<BaroquenNote> Place(IReadOnlyList<BaroquenNote> entry, Instrument targetInstrument);
}
