using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class EnsureInstrumentRange(CompositionConfiguration configuration) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        foreach (var note in nextChord.Notes)
        {
            if (!configuration.IsNoteInInstrumentRange(note.Instrument, note.Raw))
            {
                return false;
            }
        }

        return true;
    }
}
