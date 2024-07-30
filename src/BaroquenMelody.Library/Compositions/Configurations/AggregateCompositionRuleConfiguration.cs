using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Compositions.Rules.Enums;

namespace BaroquenMelody.Library.Compositions.Configurations;

internal sealed record AggregateCompositionRuleConfiguration(ISet<CompositionRuleConfiguration> Configurations)
{
    public static AggregateCompositionRuleConfiguration Default { get; } = new(
        EnumUtils<ConfigurableCompositionRule>
            .AsEnumerable()
            .Select(compositionRule => new CompositionRuleConfiguration(compositionRule))
            .ToHashSet()
    );
}
