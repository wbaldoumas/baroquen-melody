using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies;

/// <inheritdoc cref="IInputPolicy{T}"/>
internal sealed class IsRepeatedNote : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) =>
        item.NextBeat is not null &&
        item.CurrentBeat[item.Voice].Raw == item.NextBeat![item.Voice].Raw
            ? InputPolicyResult.Continue
            : InputPolicyResult.Reject;
}
