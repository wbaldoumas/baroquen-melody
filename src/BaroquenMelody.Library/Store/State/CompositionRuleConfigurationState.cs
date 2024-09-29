using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Rules.Enums;
using Fluxor;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record CompositionRuleConfigurationState(IDictionary<CompositionRule, CompositionRuleConfiguration> Configurations)
{
    private static readonly FrozenDictionary<CompositionRule, CompositionRuleConfiguration> Defaults = AggregateCompositionRuleConfiguration.Default.Configurations.ToFrozenDictionary(
        configuration => configuration.Rule,
        configuration => configuration
    );

    public AggregateCompositionRuleConfiguration Aggregate => new(Configurations.Values.ToFrozenSet());

    public CompositionRuleConfigurationState()
        : this(Defaults)
    {
    }

    public CompositionRuleConfiguration? this[CompositionRule compositionRule] => Configurations.TryGetValue(compositionRule, out var configuration) ? configuration : null;
}
