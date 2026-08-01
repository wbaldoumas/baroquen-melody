using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Random;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;

/// <inheritdoc cref="IInputPolicy{T}"/>
internal sealed class WantsToOrnament(IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator, int probability = WantsToOrnament.DefaultProbability) : IInputPolicy<OrnamentationItem>
{
    internal const int DefaultProbability = 80;

    public InputPolicyResult ShouldProcess(OrnamentationItem item) => weightedRandomBooleanGenerator.IsTrue(probability) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
