using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidRepetition : ICompositionRule
{
    private const int MinimumPrecedingChords = 2;

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count < MinimumPrecedingChords)
        {
            return true;
        }

        var precedingChordsToCheck = precedingChords.Skip(precedingChords.Count - MinimumPrecedingChords).ToList();

        foreach (var note in nextChord.Notes)
        {
            foreach (var precedingChord in precedingChordsToCheck)
            {
                if (precedingChord[note.Instrument].Raw != nextChord[note.Instrument].Raw)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
