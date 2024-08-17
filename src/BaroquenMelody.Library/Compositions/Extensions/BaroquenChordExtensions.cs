using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Library.Compositions.Extensions;

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
        var notes = new List<BaroquenNote>();
        var noteChoicesToApply = new List<NoteChoice>();

        foreach (var noteChoice in chordChoice.NoteChoices)
        {
            if (chord.ContainsInstrument(noteChoice.Instrument))
            {
                noteChoicesToApply.Add(noteChoice);
            }
        }

        foreach (var noteChoice in noteChoicesToApply)
        {
            var instrument = noteChoice.Instrument;
            var note = chord[instrument];

            notes.Add(note.ApplyNoteChoice(scale, noteChoice, noteTimeSpan));
        }

        return new BaroquenChord(notes);
    }

    public static bool InstrumentsMoveInParallel(this BaroquenChord precedingChord, BaroquenChord nextChord, Instrument instrumentA, Instrument insrumentB)
    {
        var lastNoteA = precedingChord[instrumentA];
        var lastNoteB = precedingChord[insrumentB];
        var nextNoteA = nextChord[instrumentA];
        var nextNoteB = nextChord[insrumentB];

        var noteMotionA = NoteMotionExtensions.FromNotes(lastNoteA, nextNoteA);
        var noteMotionB = NoteMotionExtensions.FromNotes(lastNoteB, nextNoteB);

        return noteMotionA != NoteMotion.Oblique && noteMotionA == noteMotionB;
    }
}
