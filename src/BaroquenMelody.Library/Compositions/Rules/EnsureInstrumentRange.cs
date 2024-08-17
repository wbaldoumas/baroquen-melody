using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Rules;

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
