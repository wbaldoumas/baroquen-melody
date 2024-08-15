using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Reducers;

public static class CompositionOrnamentationConfigurationReducers
{
    [ReducerMethod]
    public static CompositionOrnamentationConfigurationState ReduceUpdateCompositionOrnamentationConfiguration(
        CompositionOrnamentationConfigurationState state,
        UpdateCompositionOrnamentationConfiguration action)
    {
        var configurations = new Dictionary<OrnamentationType, OrnamentationConfiguration>(state.Configurations)
        {
            [action.OrnamentationType] = new(action.OrnamentationType, action.IsEnabled, action.Probability)
        };

        return new CompositionOrnamentationConfigurationState(configurations);
    }
}
