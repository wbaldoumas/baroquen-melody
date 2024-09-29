using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Rules.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Configurations;

public sealed record AggregateCompositionRuleConfiguration(ISet<CompositionRuleConfiguration> Configurations)
{
    public static AggregateCompositionRuleConfiguration Default { get; } = new(
        EnumUtils<CompositionRule>
            .AsEnumerable()
            .Select(static compositionRule => new CompositionRuleConfiguration(compositionRule))
            .ToFrozenSet()
    );
}
