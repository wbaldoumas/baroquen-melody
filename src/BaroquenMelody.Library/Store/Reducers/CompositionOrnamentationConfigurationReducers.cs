using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;
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
            [action.OrnamentationType] = new(action.OrnamentationType, action.Status, action.Probability)
        };

        return new CompositionOrnamentationConfigurationState(configurations);
    }
}
