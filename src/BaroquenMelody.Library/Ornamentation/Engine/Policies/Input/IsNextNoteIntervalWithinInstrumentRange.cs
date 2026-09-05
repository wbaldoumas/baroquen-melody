using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

internal sealed class IsNextNoteIntervalWithinInstrumentRange(CompositionConfiguration compositionConfiguration, int interval) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        var nextNote = item.NextBeat![item.Instrument];
        var noteIndex = compositionConfiguration.Scale.IndexOf(nextNote);

        var notes = compositionConfiguration.Scale.GetNotes();
        var intervalNoteIndex = noteIndex + interval;

        // An interval that leaves the scale's note list cannot be within the instrument's range.
        if (noteIndex < 0 || intervalNoteIndex < 0 || intervalNoteIndex >= notes.Count)
        {
            return InputPolicyResult.Reject;
        }

        var intervalNote = notes[intervalNoteIndex];

        return compositionConfiguration.IsNoteInInstrumentRange(item.Instrument, intervalNote) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
