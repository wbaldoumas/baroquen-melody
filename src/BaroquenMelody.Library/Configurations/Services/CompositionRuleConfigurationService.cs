using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Configurations.Services;

internal sealed class CompositionRuleConfigurationService(
    IDispatcher dispatcher,
    IState<CompositionRuleConfigurationState> state
) : ICompositionRuleConfigurationService
{
    private const int MinStrictness = 10;

    private const int MaxStrictness = 100;

    private static readonly FrozenSet<CompositionRule> _configurableOrnamentations = AggregateCompositionRuleConfiguration
        .Default
        .Configurations
        .Select(configuration => configuration.Rule)
        .ToFrozenSet();

    public IEnumerable<CompositionRule> ConfigurableCompositionRules => _configurableOrnamentations;

    public void ConfigureDefaults()
    {
        var compositionRuleConfigurationsByRuleType = AggregateCompositionRuleConfiguration.Default.Configurations.ToDictionary(
            configuration => configuration.Rule,
            configuration => new CompositionRuleConfiguration(configuration.Rule, configuration.Status, configuration.Strictness)
        );

        dispatcher.Dispatch(new BatchUpdateCompositionRuleConfiguration(compositionRuleConfigurationsByRuleType));
    }

    public void Randomize()
    {
        var compositionRuleConfigurationsByRuleType = new Dictionary<CompositionRule, CompositionRuleConfiguration>();

        foreach (var configuration in AggregateCompositionRuleConfiguration.Default.Configurations)
        {
            if (state.Value.Configurations[configuration.Rule].IsFrozen)
            {
                compositionRuleConfigurationsByRuleType.Add(
                    configuration.Rule,
                    state.Value.Configurations[configuration.Rule]
                );

                continue;
            }

            var status = state.Value.Configurations[configuration.Rule].Status;
            var strictness = ThreadLocalRandom.Next(MinStrictness, MaxStrictness + 1);

            compositionRuleConfigurationsByRuleType.Add(
                configuration.Rule,
                new CompositionRuleConfiguration(configuration.Rule, status, strictness)
            );
        }

        dispatcher.Dispatch(new BatchUpdateCompositionRuleConfiguration(compositionRuleConfigurationsByRuleType));
    }
}
