using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Dynamics.Engine.Policies.Input;

internal sealed class HasProcessedCurrentBeat : IInputPolicy<DynamicsApplicationItem>
{
    public InputPolicyResult ShouldProcess(DynamicsApplicationItem item) => item.HasProcessedCurrentBeat ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
