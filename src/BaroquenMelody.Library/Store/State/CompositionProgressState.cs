using BaroquenMelody.Library.Compositions.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record CompositionProgressState(ISet<CompositionStep> CompletedSteps, CompositionStep CurrentStep)
{
    public CompositionProgressState()
        : this(new HashSet<CompositionStep>(), CompositionStep.Waiting)
    {
    }
}
