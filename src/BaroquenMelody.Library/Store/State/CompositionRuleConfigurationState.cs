using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Rules.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record CompositionRuleConfigurationState(IDictionary<CompositionRule, CompositionRuleConfiguration> Configurations)
{
    private static readonly IDictionary<CompositionRule, CompositionRuleConfiguration> Defaults = AggregateCompositionRuleConfiguration.Default.Configurations.ToDictionary(
        configuration => configuration.Rule,
        configuration => configuration
    );

    public AggregateCompositionRuleConfiguration Aggregate => new(Configurations.Values.ToHashSet());

    public CompositionRuleConfigurationState()
        : this(Defaults)
    {
    }

    public CompositionRuleConfiguration? this[CompositionRule compositionRule] => Configurations.TryGetValue(compositionRule, out var configuration) ? configuration : null;
}
