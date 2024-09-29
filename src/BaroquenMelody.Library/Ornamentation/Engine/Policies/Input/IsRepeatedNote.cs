using Atrea.PolicyEngine.Policies.Input;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

/// <inheritdoc cref="IInputPolicy{T}"/>
internal sealed class IsRepeatedNote : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) =>
        item.NextBeat is not null &&
        item.CurrentBeat.ContainsInstrument(item.Instrument) &&
        item.CurrentBeat[item.Instrument].Raw == item.NextBeat![item.Instrument].Raw
            ? InputPolicyResult.Continue
            : InputPolicyResult.Reject;
}
