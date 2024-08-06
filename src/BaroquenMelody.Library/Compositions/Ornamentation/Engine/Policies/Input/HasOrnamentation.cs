using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

internal sealed class HasOrnamentation : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) => item.CurrentBeat[item.Voice].HasOrnamentations ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
