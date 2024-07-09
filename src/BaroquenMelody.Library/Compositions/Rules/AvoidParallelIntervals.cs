using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums.Extensions;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidParallelIntervals(Interval targetInterval) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        return !HasParallelIntervals(precedingChords[^1], nextChord);
    }

    private bool HasParallelIntervals(BaroquenChord lastChord, BaroquenChord nextChord)
    {
        var parallelPerfectVoicePairs = lastChord.Notes
            .SelectMany(note => lastChord.Notes
                .Where(otherNote => note != otherNote && IntervalExtensions.FromNotes(note, otherNote) == targetInterval)
                .Select(otherNote => (note.Voice, otherNote.Voice)));

        foreach (var (voiceA, voiceB) in parallelPerfectVoicePairs)
        {
            var nextNoteA = nextChord[voiceA];
            var nextNoteB = nextChord[voiceB];

            if (IntervalExtensions.FromNotes(nextNoteA, nextNoteB) == targetInterval && HaveSameMotion(lastChord, nextChord, voiceA, voiceB))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HaveSameMotion(BaroquenChord lastChord, BaroquenChord nextChord, Voice voiceA, Voice voiceB)
    {
        var lastNoteA = lastChord[voiceA];
        var lastNoteB = lastChord[voiceB];
        var nextNoteA = nextChord[voiceA];
        var nextNoteB = nextChord[voiceB];

        var noteMotionA = NoteMotionExtensions.FromNotes(lastNoteA, nextNoteA);
        var noteMotionB = NoteMotionExtensions.FromNotes(lastNoteB, nextNoteB);

        return noteMotionA != NoteMotion.Oblique && noteMotionB != NoteMotion.Oblique && noteMotionA == noteMotionB;
    }
}
