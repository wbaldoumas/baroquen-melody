using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using LazyCart;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

internal sealed class AvoidParallelFifths(CompositionConfiguration compositionConfiguration) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count == 0)
        {
            return true;
        }

        var precedingChord = precedingChords[^1];

        var voices = compositionConfiguration.VoiceConfigurations.Select(voiceConfiguration => voiceConfiguration.Voice).ToList();

        var voiceCombinations = new LazyCartesianProduct<Voice, Voice>(voices, voices);

        for (var i = 0; i < voiceCombinations.Size; ++i)
        {
            var (voice1, voice2) = voiceCombinations[i];

            if (voice1 == voice2)
            {
                continue;
            }

            var precedingNote1 = precedingChord[voice1];
            var precedingNote2 = precedingChord[voice2];

            var currentNote1 = nextChord[voice1];
            var currentNote2 = nextChord[voice2];

            var direction1 = precedingNote1.GetDirectionTo(currentNote1);
            var direction2 = precedingNote2.GetDirectionTo(currentNote2);

            if (direction1 == NoteMotion.Oblique || direction2 == NoteMotion.Oblique)
            {
                continue;
            }

            if (direction1 != direction2)
            {
                continue;
            }

            if (precedingNote1.IsPerfectFifthWith(precedingNote2) && currentNote1.IsPerfectFifthWith(currentNote2))
            {
                return false;
            }
        }

        return true;
    }
}
