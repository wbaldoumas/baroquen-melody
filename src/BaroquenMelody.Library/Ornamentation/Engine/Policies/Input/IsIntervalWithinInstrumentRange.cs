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
        var nextNote = notes[noteIndex + interval];

        return compositionConfiguration.IsNoteInInstrumentRange(item.Instrument, nextNote) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
