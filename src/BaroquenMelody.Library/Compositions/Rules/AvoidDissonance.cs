using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidDissonance : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var notes = nextChord.Notes.DistinctBy(static note => note.NoteName).ToList();

        foreach (var note in notes)
        {
            foreach (var otherNote in notes)
            {
                if (note == otherNote)
                {
                    continue;
                }

                if (note.IsDissonantWith(otherNote))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
