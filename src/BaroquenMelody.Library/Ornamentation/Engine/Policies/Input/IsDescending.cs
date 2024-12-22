using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

internal sealed class IsDescending : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        var currentNote = item.CurrentBeat[item.Instrument];
        var nextNote = item.NextBeat?[item.Instrument];

        return currentNote > nextNote ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
