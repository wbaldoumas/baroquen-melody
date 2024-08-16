using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Rules.Enums;
using Fluxor;

namespace BaroquenMelody.Library.Store.State;

[FeatureState]
public sealed record CompositionRuleConfigurationState(IDictionary<CompositionRule, CompositionRuleConfiguration> Configurations)
{
    public AggregateCompositionRuleConfiguration Aggregate => new(Configurations.Values.ToHashSet());

    public CompositionRuleConfigurationState()
        : this(new Dictionary<CompositionRule, CompositionRuleConfiguration>())
    {
    }

    public CompositionRuleConfiguration? this[CompositionRule compositionRule] => Configurations.TryGetValue(compositionRule, out var configuration) ? configuration : null;
}
