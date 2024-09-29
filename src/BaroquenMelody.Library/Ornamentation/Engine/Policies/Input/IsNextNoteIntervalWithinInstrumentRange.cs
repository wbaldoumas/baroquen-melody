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
        var intervalNote = notes[noteIndex + interval];

        return compositionConfiguration.IsNoteInInstrumentRange(item.Instrument, intervalNote) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
