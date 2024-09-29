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
