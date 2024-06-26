using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

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

        return nextChord.Notes
            .Select(note => note.Voice)
            .All(voice => precedingChordsToCheck.Exists(precedingChord => precedingChord[voice].Raw != nextChord[voice].Raw));
    }
}
