using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Viewpoints;

namespace BaroquenMelody.Library.Rules.Melodic;

/// <inheritdoc cref="IMelodicCompositionRule"/>
/// <remarks>
///     A leading tone which a voice ascended into must resolve upward: the note after the seventh scale degree
///     must continue above it.
/// </remarks>
internal sealed class HandleAscendingSeventh(CompositionConfiguration compositionConfiguration) : IMelodicCompositionRule
{
    public bool Evaluate(in MelodicLine line)
    {
        var lastNote = line.PrecedingNote(1);

        if (lastNote is null || lastNote.NoteName != compositionConfiguration.Scale.LeadingTone)
        {
            return true;
        }

        var secondLastNote = line.PrecedingNote(2);

        if (secondLastNote is null || secondLastNote.NoteNumber >= lastNote.NoteNumber)
        {
            return true;
        }

        return line.NextNote.NoteNumber > lastNote.NoteNumber;
    }
}
