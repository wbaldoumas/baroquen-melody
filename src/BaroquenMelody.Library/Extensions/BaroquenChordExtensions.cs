using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Enums.Extensions;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Extensions;

/// <summary>
///     A home for extension methods for <see cref="BaroquenChord"/>.
/// </summary>
internal static class BaroquenChordExtensions
{
    public static BaroquenChord ApplyChordChoice(
        this BaroquenChord chord,
        BaroquenScale scale,
        ChordChoice chordChoice,
        MusicalTimeSpan noteTimeSpan)
    {
        var notes = new List<BaroquenNote>(chordChoice.NoteChoices.Count);

        foreach (var noteChoice in chordChoice.NoteChoices)
        {
            if (!chord.ContainsInstrument(noteChoice.Instrument))
            {
                continue;
            }

            notes.Add(chord[noteChoice.Instrument].ApplyNoteChoice(scale, noteChoice, noteTimeSpan));
        }

        return new BaroquenChord(notes);
    }

    /// <summary>
    ///     Retrieves the note of the highest-register instrument voiced in the chord.
    /// </summary>
    /// <param name="chord">The chord to find the highest voiced note of.</param>
    /// <param name="registerOrderedInstruments">The instruments ordered from highest register to lowest.</param>
    /// <returns>The highest voiced note, or <see langword="null"/> when the chord voices none of the instruments.</returns>
    public static BaroquenNote? GetHighestVoicedNote(this BaroquenChord chord, IReadOnlyList<Instrument> registerOrderedInstruments)
    {
        foreach (var instrument in registerOrderedInstruments)
        {
            if (chord.ContainsInstrument(instrument))
            {
                return chord[instrument];
            }
        }

        return null;
    }

    /// <summary>
    ///     Retrieves the note of the lowest-register instrument voiced in the chord (its bass voice).
    /// </summary>
    /// <param name="chord">The chord to find the lowest voiced note of.</param>
    /// <param name="registerOrderedInstruments">The instruments ordered from highest register to lowest.</param>
    /// <returns>The lowest voiced note, or <see langword="null"/> when the chord voices none of the instruments.</returns>
    public static BaroquenNote? GetLowestVoicedNote(this BaroquenChord chord, IReadOnlyList<Instrument> registerOrderedInstruments)
    {
        for (var instrumentIndex = registerOrderedInstruments.Count - 1; instrumentIndex >= 0; --instrumentIndex)
        {
            if (chord.ContainsInstrument(registerOrderedInstruments[instrumentIndex]))
            {
                return chord[registerOrderedInstruments[instrumentIndex]];
            }
        }

        return null;
    }

    public static bool InstrumentsMoveInParallel(this BaroquenChord precedingChord, BaroquenChord nextChord, Instrument instrumentA, Instrument instrumentB)
    {
        var lastNoteA = precedingChord[instrumentA];
        var lastNoteB = precedingChord[instrumentB];
        var nextNoteA = nextChord[instrumentA];
        var nextNoteB = nextChord[instrumentB];

        var noteMotionA = NoteMotionExtensions.FromNotes(lastNoteA, nextNoteA);
        var noteMotionB = NoteMotionExtensions.FromNotes(lastNoteB, nextNoteB);

        return noteMotionA != NoteMotion.Oblique && noteMotionA == noteMotionB;
    }
}
