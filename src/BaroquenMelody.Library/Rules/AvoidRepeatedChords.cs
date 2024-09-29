using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;

namespace BaroquenMelody.Library.Rules;

internal sealed class AvoidRepeatedChords(IChordNumberIdentifier chordNumberIdentifier) : ICompositionRule
{
    private const int MinimumPrecedingChords = 2;

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count < MinimumPrecedingChords)
        {
            return true;
        }

        var nextChordNumber = chordNumberIdentifier.IdentifyChordNumber(nextChord);

        foreach (var precedingChord in precedingChords.Skip(precedingChords.Count - MinimumPrecedingChords))
        {
            if (chordNumberIdentifier.IdentifyChordNumber(precedingChord) != nextChordNumber)
            {
                return true;
            }
        }

        return false;
    }
}
