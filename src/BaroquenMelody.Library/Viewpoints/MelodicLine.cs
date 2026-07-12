using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Viewpoints;

/// <summary>
///     A melodic-viewpoint read model: the horizontal line one voice traces through the preceding chords up to a
///     proposed next note. The projection is chord-aligned: preceding notes are indexed by chord position, so a
///     voice which is absent from some chord yields no note at that position rather than collapsing the gap.
/// </summary>
/// <param name="precedingChords">The chords which precede the proposed next note.</param>
/// <param name="nextNote">The proposed next note, which identifies the voice by its instrument.</param>
internal readonly struct MelodicLine(IReadOnlyList<BaroquenChord> precedingChords, BaroquenNote nextNote)
{
    /// <summary>
    ///     The proposed next note of the line.
    /// </summary>
    public BaroquenNote NextNote { get; } = nextNote;

    /// <summary>
    ///     The instrument whose line this is.
    /// </summary>
    public Instrument Instrument => NextNote.Instrument;

    /// <summary>
    ///     Retrieves the note this voice plays some number of chords back from the proposed next note.
    /// </summary>
    /// <param name="chordsBack">How many chords back to look; one is the immediately preceding chord.</param>
    /// <returns>The note, or <see langword="null"/> when there is no such chord or the voice is absent from it.</returns>
    public BaroquenNote? PrecedingNote(int chordsBack)
    {
        if (chordsBack <= 0 || chordsBack > precedingChords.Count)
        {
            return null;
        }

        var precedingChord = precedingChords[precedingChords.Count - chordsBack];

        return precedingChord.ContainsInstrument(Instrument) ? precedingChord[Instrument] : null;
    }
}
