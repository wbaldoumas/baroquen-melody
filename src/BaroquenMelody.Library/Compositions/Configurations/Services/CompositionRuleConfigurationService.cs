﻿using BaroquenMelody.Library.Compositions.Rules.Enums;
using BaroquenMelody.Library.Infrastructure.Random;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

internal sealed class CompositionRuleConfigurationService(IDispatcher dispatcher) : ICompositionRuleConfigurationService
{
    private static readonly FrozenSet<CompositionRule> _configurableOrnamentations = AggregateCompositionRuleConfiguration
        .Default
        .Configurations
        .Select(configuration => configuration.Rule)
        .ToFrozenSet();

    public IEnumerable<CompositionRule> ConfigurableCompositionRules => _configurableOrnamentations;

    public void ConfigureDefaults()
    {
        foreach (var configuration in AggregateCompositionRuleConfiguration.Default.Configurations)
        {
            dispatcher.Dispatch(new UpdateCompositionRuleConfiguration(configuration.Rule, configuration.IsEnabled, configuration.Strictness));
        }
    }

    public void Randomize()
    {
        foreach (var configuration in AggregateCompositionRuleConfiguration.Default.Configurations)
        {
            var isEnabled = ThreadLocalRandom.Next() % 2 == 0;
            var strictness = ThreadLocalRandom.Next(0, 101);

            dispatcher.Dispatch(new UpdateCompositionRuleConfiguration(configuration.Rule, isEnabled, strictness));
        }
    }
}
