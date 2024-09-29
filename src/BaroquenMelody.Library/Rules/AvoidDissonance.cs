using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Extensions;

namespace BaroquenMelody.Library.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AvoidDissonance : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        foreach (var note in nextChord.Notes)
        {
            foreach (var otherNote in nextChord.Notes)
            {
                if (note.NoteName == otherNote.NoteName)
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
