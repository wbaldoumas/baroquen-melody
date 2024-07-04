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

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (var note in nextChord.Notes)
        {
            if (!precedingChordsToCheck.Exists(precedingChord => precedingChord[note.Voice].Raw != nextChord[note.Voice].Raw))
            {
                return false;
            }
        }

        return true;
    }
}
