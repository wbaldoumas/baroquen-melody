using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Reducers;

public static class CompositionConfigurationReducers
{
    [ReducerMethod]
    public static CompositionConfigurationState ReduceUpdateCompositionConfiguration(CompositionConfigurationState state, UpdateCompositionConfiguration action) => new(
        action.RootNote,
        action.Mode,
        action.Meter,
        action.CompositionLength,
        action.Tempo,
        action.Form
    );

    [ReducerMethod]
    public static CompositionConfigurationState ReduceLoadCompositionConfiguration(CompositionConfigurationState state, LoadCompositionConfiguration action) => new(
        action.CompositionConfiguration.Tonic,
        action.CompositionConfiguration.Mode,
        action.CompositionConfiguration.Meter,
        action.CompositionConfiguration.MinimumMeasures,
        action.CompositionConfiguration.Tempo,
        action.CompositionConfiguration.GroundBassConfiguration?.Enabled == true ? CompositionForm.GroundBass : CompositionForm.Fugue
    );
}
