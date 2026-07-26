using BaroquenMelody.Library.Enums;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Forms;

/// <summary>
///     The concrete plan for a ground bass composition: the chosen pattern rendered into the bass instrument's
///     register, and how many times the ground states before the final cadence.
/// </summary>
/// <param name="Pattern"> The chosen ground bass pattern. </param>
/// <param name="BassInstrument"> The instrument that carries the ground (the lowest configured voice). </param>
/// <param name="BassNotes"> The pattern rendered into the bass register, one note per ground note. </param>
/// <param name="StatementCount"> How many times the ground states, including the opening solo statement. </param>
/// <param name="MeasuresPerStatement"> How many measures one statement of the ground spans. </param>
/// <remarks>
///     This is the seam where a general tonal plan will land: statements are the composition's sections, all
///     in the home key here, and modulation later becomes re-rendering a statement's offsets against a
///     section-specific scale.
/// </remarks>
internal sealed record GroundBassPlan(
    GroundBassPattern Pattern,
    Instrument BassInstrument,
    IReadOnlyList<Note> BassNotes,
    int StatementCount,
    int MeasuresPerStatement)
{
    /// <summary>
    ///     How many composition slots each ground note spans: the onset slot, searched against the pinned bass
    ///     note, and one held slot duplicated from it. A slot is a half note in 4/4 and a full notated bar in
    ///     3/4, so the ground treads at the same slow pace in either meter.
    /// </summary>
    public const int SlotsPerGroundNote = 2;
}
