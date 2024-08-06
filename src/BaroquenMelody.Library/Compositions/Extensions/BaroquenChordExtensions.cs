using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;

namespace BaroquenMelody.Library.Compositions.Extensions;

/// <summary>
///     A home for extension methods for <see cref="BaroquenChord"/>.
/// </summary>
internal static class BaroquenChordExtensions
{
    public static BaroquenChord ApplyChordChoice(this BaroquenChord chord, BaroquenScale scale, ChordChoice chordChoice)
    {
        var notes = new List<BaroquenNote>();
        var noteChoicesToApply = new List<NoteChoice>();

        foreach (var noteChoice in chordChoice.NoteChoices)
        {
            if (chord.ContainsVoice(noteChoice.Voice))
            {
                noteChoicesToApply.Add(noteChoice);
            }
        }

        foreach (var noteChoice in noteChoicesToApply)
        {
            var voice = noteChoice.Voice;
            var note = chord[voice];

            notes.Add(note.ApplyNoteChoice(scale, noteChoice));
        }

        return new BaroquenChord(notes);
    }

    public static bool VoicesMoveInParallel(this BaroquenChord precedingChord, BaroquenChord nextChord, Voice voiceA, Voice voiceB)
    {
        var lastNoteA = precedingChord[voiceA];
        var lastNoteB = precedingChord[voiceB];
        var nextNoteA = nextChord[voiceA];
        var nextNoteB = nextChord[voiceB];

        var noteMotionA = NoteMotionExtensions.FromNotes(lastNoteA, nextNoteA);
        var noteMotionB = NoteMotionExtensions.FromNotes(lastNoteB, nextNoteB);

        return noteMotionA != NoteMotion.Oblique && noteMotionA == noteMotionB;
    }
}
