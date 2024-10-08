using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Reducers;

public static class CompositionRuleConfigurationReducers
{
    [ReducerMethod]
    public static CompositionRuleConfigurationState ReduceUpdateCompositionRuleConfiguration(CompositionRuleConfigurationState state, UpdateCompositionRuleConfiguration action)
    {
        var configurations = new Dictionary<CompositionRule, CompositionRuleConfiguration>(state.Configurations)
        {
            [action.CompositionRule] = new(action.CompositionRule, action.Status, action.Strictness)
        };

        return new CompositionRuleConfigurationState(configurations);
    }

    [ReducerMethod]
    public static CompositionRuleConfigurationState ReduceBatchUpdateCompositionRuleConfiguration(CompositionRuleConfigurationState state, BatchUpdateCompositionRuleConfiguration action) => new(action.Configurations);
}
