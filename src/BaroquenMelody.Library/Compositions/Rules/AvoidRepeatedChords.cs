using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.MusicTheory;

namespace BaroquenMelody.Library.Compositions.Rules;

internal sealed class AvoidRepeatedChords(IChordNumberIdentifier chordNumberIdentifier) : ICompositionRule
{
    private const int MinimumPrecedingChords = 2;

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count < MinimumPrecedingChords)
        {
            return true;
        }

        var precedingChordsToCheck = precedingChords.Skip(precedingChords.Count - MinimumPrecedingChords).ToList();
        var nextChordNumber = chordNumberIdentifier.IdentifyChordNumber(nextChord);

        foreach (var precedingChord in precedingChordsToCheck)
        {
            if (chordNumberIdentifier.IdentifyChordNumber(precedingChord) != nextChordNumber)
            {
                return true;
            }
        }

        return false;
    }
}
