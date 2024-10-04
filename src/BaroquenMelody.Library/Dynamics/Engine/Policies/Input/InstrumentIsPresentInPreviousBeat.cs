using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Dynamics.Engine.Policies.Input;

internal sealed class InstrumentIsPresentInPreviousBeat : IInputPolicy<DynamicsApplicationItem>
{
    public InputPolicyResult ShouldProcess(DynamicsApplicationItem item) => item.PrecedingBeats.Count > 0 && item.PrecedingBeats[^1].ContainsInstrument(item.Instrument)
        ? InputPolicyResult.Continue
        : InputPolicyResult.Reject;
}
