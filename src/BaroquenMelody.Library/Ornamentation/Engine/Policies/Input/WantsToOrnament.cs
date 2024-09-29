using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

/// <inheritdoc cref="IInputPolicy{T}"/>
internal sealed class WantsToOrnament(IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator, int probability = 80) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) => weightedRandomBooleanGenerator.IsTrue(probability) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
