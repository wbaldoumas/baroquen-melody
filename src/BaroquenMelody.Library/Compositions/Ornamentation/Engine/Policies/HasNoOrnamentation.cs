using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies;

internal sealed class HasNoOrnamentation : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) => item.CurrentBeat[item.Voice].HasOrnamentations ? InputPolicyResult.Reject : InputPolicyResult.Continue;
}
