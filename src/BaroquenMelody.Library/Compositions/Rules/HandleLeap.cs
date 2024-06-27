using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Rules.Validators;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class HandleLeap(CompositionConfiguration compositionConfiguration, ILeapResolutionValidator leapResolutionValidator) : ICompositionRule
{
    private const int MinimumPrecedingChords = 2;

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        if (precedingChords.Count < MinimumPrecedingChords)
        {
            return true;
        }

        var nextToLastChord = precedingChords[^2];

        if (nextToLastChord == nextChord)
        {
            return true;
        }

        var lastChord = precedingChords[^1];
        var notes = compositionConfiguration.Scale.GetNotes();

        return leapResolutionValidator.HasValidLeapResolution(nextToLastChord, lastChord, nextChord, notes);
    }
}
