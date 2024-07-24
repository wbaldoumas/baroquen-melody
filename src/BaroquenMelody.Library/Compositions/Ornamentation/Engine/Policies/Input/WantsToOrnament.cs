using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;

/// <inheritdoc cref="IInputPolicy{T}"/>
internal sealed class WantsToOrnament(IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator, int Probability = 80) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) => weightedRandomBooleanGenerator.IsTrue(Probability) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
