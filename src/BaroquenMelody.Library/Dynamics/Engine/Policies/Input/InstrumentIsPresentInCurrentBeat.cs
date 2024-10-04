using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Dynamics.Engine.Policies.Input;

internal sealed class InstrumentIsPresentInCurrentBeat : IInputPolicy<DynamicsApplicationItem>
{
    public InputPolicyResult ShouldProcess(DynamicsApplicationItem item) => item.CurrentBeat.ContainsInstrument(item.Instrument) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
