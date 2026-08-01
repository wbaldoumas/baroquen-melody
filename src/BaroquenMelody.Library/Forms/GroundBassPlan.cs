using BaroquenMelody.Library.Enums;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Forms;

/// <summary>
///     The concrete plan for a ground bass composition: the chosen pattern rendered into the bass instrument's
///     register, and how many times the ground states before the final cadence.
/// </summary>
/// <param name="Pattern"> The chosen ground bass pattern. </param>
/// <param name="BassInstrument"> The instrument that carries the ground (the lowest configured voice). </param>
/// <param name="BassNotes"> The pattern rendered into the bass register against the home key, one note per ground note. </param>
/// <param name="StatementCount"> How many times the ground states, including the opening solo statement. </param>
/// <param name="MeasuresPerStatement"> How many measures one statement of the ground spans. </param>
/// <param name="Sections"> The tonal plan: contiguous statement runs each carrying one key and that key's rendering of the ground. </param>
/// <remarks>
///     The tonal plan lands here: statements are the composition's sections. A home-only plan carries a
///     single section spanning every statement, so every section-aware consumer degenerates to the
///     pre-modulation behavior; a modulating plan renders the same offsets against the relative key's scale
///     for its middle sections. The opening announcement and the final cadence always sit in home-key
///     sections (the planner's lead and tail floors), so the solo strip and the close read
///     <see cref="BassNotes"/> directly.
/// </remarks>
internal sealed record GroundBassPlan(
    GroundBassPattern Pattern,
    Instrument BassInstrument,
    IReadOnlyList<Note> BassNotes,
    int StatementCount,
    int MeasuresPerStatement,
    IReadOnlyList<TonalSection> Sections)
{
    /// <summary>
    ///     Retrieve the tonal section a statement belongs to. Sections partition the statement indices in
    ///     ascending order, so the first section whose span reaches the statement is its owner.
    /// </summary>
    /// <param name="statementIndex"> The zero-based statement index. </param>
    /// <returns> The section spanning the statement. </returns>
    public TonalSection SectionForStatement(int statementIndex) => Sections.First(section => statementIndex <= section.LastStatement);

    /// <summary>
    ///     How many composition slots each ground note spans: the onset slot, searched against the pinned bass
    ///     note, and one held slot duplicated from it. A slot is a half note in 4/4 and a full notated bar in
    ///     3/4, so the ground treads at the same slow pace in either meter.
    /// </summary>
    public const int SlotsPerGroundNote = 2;
}
