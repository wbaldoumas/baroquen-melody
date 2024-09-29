using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Reducers;

public static class CompositionProgressReducers
{
    [ReducerMethod]
    public static CompositionProgressState ReduceUpdateCompositionProgressAction(CompositionProgressState state, ProgressCompositionStep action)
    {
        var completedSteps = new HashSet<CompositionStep>(state.CompletedSteps)
        {
            state.CurrentStep
        };

        return state with { CompletedSteps = completedSteps, CurrentStep = action.Step };
    }

    [ReducerMethod]
    public static CompositionProgressState ReduceProgressCompositionThemeProgressAction(CompositionProgressState state, ProgressCompositionThemeProgress action)
    {
        return state with { ThemeProgress = action.Progress };
    }

    [ReducerMethod]
    public static CompositionProgressState ReduceProgressCompositionBodyProgressAction(CompositionProgressState state, ProgressCompositionBodyProgress action)
    {
        return state with { BodyProgress = action.Progress };
    }

    [ReducerMethod]
    public static CompositionProgressState ReduceProgressCompositionEndingProgressAction(CompositionProgressState state, ProgressCompositionEndingProgress action)
    {
        return state with { EndingProgress = action.Progress };
    }

    [ReducerMethod]
    public static CompositionProgressState ReduceResetCompositionProgressAction(CompositionProgressState state, ResetCompositionProgress action)
    {
        return new CompositionProgressState();
    }

    [ReducerMethod]
    public static CompositionProgressState ReduceMarkCompositionFailed(CompositionProgressState state, MarkCompositionFailed _)
    {
        return new CompositionProgressState { CurrentStep = CompositionStep.Failed };
    }
}
