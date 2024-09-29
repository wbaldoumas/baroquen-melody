using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

internal sealed class HasNextBeat : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) => item.NextBeat is not null && item.NextBeat.ContainsInstrument(item.Instrument) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
