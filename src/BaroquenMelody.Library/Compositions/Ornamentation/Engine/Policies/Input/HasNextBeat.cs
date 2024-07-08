using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class HasNextBeat : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) => item.NextBeat is not null ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
