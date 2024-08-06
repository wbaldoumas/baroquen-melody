using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
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
        foreach (var (voiceA, voiceB) in GetParallelPerfectVoicePairs(lastChord))
        {
            var nextNoteA = nextChord[voiceA];
            var nextNoteB = nextChord[voiceB];

            if (IntervalExtensions.FromNotes(nextNoteA, nextNoteB) == targetInterval && lastChord.VoicesMoveInParallel(nextChord, voiceA, voiceB))
            {
                return true;
            }
        }

        return false;
    }

    private List<(Voice, Voice)> GetParallelPerfectVoicePairs(BaroquenChord lastChord)
    {
        var parallelPerfectVoicePairs = new List<(Voice, Voice)>();

        foreach (var note in lastChord.Notes)
        {
            foreach (var otherNote in lastChord.Notes)
            {
                if (note == otherNote)
                {
                    continue;
                }

                if (IntervalExtensions.FromNotes(note, otherNote) == targetInterval)
                {
                    parallelPerfectVoicePairs.Add((note.Voice, otherNote.Voice));
                }
            }
        }

        return parallelPerfectVoicePairs;
    }
}
