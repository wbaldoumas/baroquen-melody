using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Rules;

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

        foreach (var precedingChord in precedingChords.Skip(precedingChords.Count - MinimumPrecedingChords))
        {
            foreach (var note in nextChord.Notes)
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
