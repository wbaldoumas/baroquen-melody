using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Reducers;

public static class CompositionProgressReducers
{
    [ReducerMethod]
    public static CompositionProgressState ReduceUpdateCompositionProgressAction(CompositionProgressState state, ProgressCompositionStepAction action)
    {
        var completedSteps = new HashSet<CompositionStep>(state.CompletedSteps)
        {
            state.CurrentStep
        };

        return new CompositionProgressState(completedSteps, action.Step);
    }
}
