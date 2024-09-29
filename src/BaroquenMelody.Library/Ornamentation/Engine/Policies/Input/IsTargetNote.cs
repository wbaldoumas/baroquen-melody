using Atrea.PolicyEngine.Policies.Input;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

internal sealed class IsTargetNote(NoteName noteName) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) => item.CurrentBeat[item.Instrument].NoteName == noteName ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
