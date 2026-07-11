using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.Viewpoints;

namespace BaroquenMelody.Library.Rules.Melodic;

/// <inheritdoc cref="IMelodicCompositionRule"/>
/// <remarks>
///     A voice must not leap (more than one scale step) between two consecutive notes which are dissonant with
///     each other.
/// </remarks>
internal sealed class AvoidDissonantLeaps(CompositionConfiguration compositionConfiguration) : IMelodicCompositionRule
{
    private const int LeapThreshold = 1;

    public bool Evaluate(in MelodicLine line)
    {
        var lastNote = line.PrecedingNote(1);

        if (lastNote is null || !lastNote.IsDissonantWith(line.NextNote))
        {
            return true;
        }

        var notes = compositionConfiguration.Scale.GetNotes();
        var lastNoteScaleIndex = notes.IndexOf(lastNote.Raw);
        var nextNoteScaleIndex = notes.IndexOf(line.NextNote.Raw);

        return Math.Abs(lastNoteScaleIndex - nextNoteScaleIndex) <= LeapThreshold;
    }
}
