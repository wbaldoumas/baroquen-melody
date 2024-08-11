using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class HasNextBeat : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) => item.NextBeat is not null && item.NextBeat.ContainsVoice(item.Voice) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
