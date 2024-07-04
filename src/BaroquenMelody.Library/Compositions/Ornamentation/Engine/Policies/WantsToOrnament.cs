using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies;

/// <inheritdoc cref="IInputPolicy{T}"/>
internal sealed class WantsToOrnament(int Probability = 50) : IInputPolicy<OrnamentationItem>
{
    public InputPolicyResult ShouldProcess(OrnamentationItem item) => WeightedRandomBooleanGenerator.Generate(Probability) ? InputPolicyResult.Continue : InputPolicyResult.Reject;
}
