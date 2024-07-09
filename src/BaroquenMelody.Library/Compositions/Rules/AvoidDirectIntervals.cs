using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums.Extensions;
using LazyCart;

namespace BaroquenMelody.Library.Compositions.Rules;

internal sealed class AvoidDirectIntervals(Interval targetInterval) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        var voices = nextChord.Notes.Select(note => note.Voice).ToArray();
        var voiceCombos = new LazyCartesianProduct<Voice, Voice>(voices, voices);
        var precedingChord = precedingChords[^1];

        for (var i = 0; i < voiceCombos.Size; ++i)
        {
            var (voiceA, voiceB) = voiceCombos[i];

            if (voiceA == voiceB)
            {
                continue;
            }

            if (HaveTargetInterval(nextChord, voiceA, voiceB, targetInterval) && HaveSameMotion(precedingChord, nextChord, voiceA, voiceB))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HaveTargetInterval(BaroquenChord nextChord, Voice voiceA, Voice voiceB, Interval targetInterval)
    {
        var nextNoteA = nextChord[voiceA];
        var nextNoteB = nextChord[voiceB];

        return IntervalExtensions.FromNotes(nextNoteA, nextNoteB) == targetInterval;
    }

    private static bool HaveSameMotion(BaroquenChord precedingChord, BaroquenChord nextChord, Voice voiceA, Voice voiceB)
    {
        var lastNoteA = precedingChord[voiceA];
        var lastNoteB = precedingChord[voiceB];
        var nextNoteA = nextChord[voiceA];
        var nextNoteB = nextChord[voiceB];

        var noteMotionA = NoteMotionExtensions.FromNotes(lastNoteA, nextNoteA);
        var noteMotionB = NoteMotionExtensions.FromNotes(lastNoteB, nextNoteB);

        return noteMotionA != NoteMotion.Oblique && noteMotionB != NoteMotion.Oblique && noteMotionA == noteMotionB;
    }
}
