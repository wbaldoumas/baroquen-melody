using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies;

internal sealed class WantsToOrnament(int Threshold = 50) : IInputPolicy<OrnamentationItem>
{
    private const int Max = 100;

    public InputPolicyResult ShouldProcess(OrnamentationItem item)
    {
        return ThreadLocalRandom.Next(Max) > Threshold ? InputPolicyResult.Continue : InputPolicyResult.Reject;
    }
}
