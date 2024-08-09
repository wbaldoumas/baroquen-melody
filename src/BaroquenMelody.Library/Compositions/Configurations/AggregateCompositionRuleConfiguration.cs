using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Compositions.Rules.Enums;

namespace BaroquenMelody.Library.Compositions.Configurations;

public sealed record AggregateCompositionRuleConfiguration(ISet<CompositionRuleConfiguration> Configurations)
{
    public static AggregateCompositionRuleConfiguration Default { get; } = new(
        EnumUtils<CompositionRule>
            .AsEnumerable()
            .Select(static compositionRule => new CompositionRuleConfiguration(compositionRule))
            .ToHashSet()
    );
}
