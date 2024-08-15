using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Reducers;

public static class CompositionConfigurationReducers
{
    [ReducerMethod]
    public static CompositionConfigurationState ReduceUpdateCompositionConfiguration(CompositionConfigurationState state, UpdateCompositionConfiguration action)
    {
        return new CompositionConfigurationState(action.Scale, action.Meter, action.CompositionLength);
    }
}
