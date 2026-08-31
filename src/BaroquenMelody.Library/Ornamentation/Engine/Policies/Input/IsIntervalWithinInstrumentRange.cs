using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

internal sealed class IsIntervalWithinInstrumentRange(CompositionConfiguration compositionConfiguration, int interval) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var noteIndex = compositionConfiguration.Scale.IndexOf(currentNote);

        var notes = compositionConfiguration.Scale.GetNotes();
        var nextNoteIndex = noteIndex + interval;

        // An interval that leaves the scale's note list cannot be within the instrument's range.
        if (noteIndex < 0 || nextNoteIndex < 0 || nextNoteIndex >= notes.Count)
        {
            return InputPolicyResult.Reject;
        }

        var nextNote = notes[nextNoteIndex];

        return compositionConfiguration.IsNoteInInstrumentRange(item.Instrument, nextNote) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
