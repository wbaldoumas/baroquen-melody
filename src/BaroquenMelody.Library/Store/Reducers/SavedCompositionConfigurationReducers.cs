using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Reducers;

public static class SavedCompositionConfigurationReducers
{
    [ReducerMethod]
    public static SavedCompositionConfigurationState ReduceUpdateLastLoadedConfigurationName(
        SavedCompositionConfigurationState state,
        UpdateLastLoadedConfigurationName action
    ) => new(action.LastLoadedConfigurationName);
}
