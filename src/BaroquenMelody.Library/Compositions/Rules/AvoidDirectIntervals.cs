using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums.Extensions;
using LazyCart;
using Interval = BaroquenMelody.Library.Compositions.MusicTheory.Enums.Interval;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidDirectIntervals(Interval targetInterval) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        var voices = nextChord.Notes.Select(static note => note.Voice).ToList();
        var voiceCombos = new LazyCartesianProduct<Voice, Voice>(voices, voices);
        var precedingChord = precedingChords[^1];

        for (var i = 0; i < voiceCombos.Size; ++i)
        {
            var (voiceA, voiceB) = voiceCombos[i];

            if (voiceA == voiceB)
            {
                continue;
            }

            if (HaveTargetInterval(nextChord, voiceA, voiceB, targetInterval) && precedingChord.VoicesMoveInParallel(nextChord, voiceA, voiceB))
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
}
