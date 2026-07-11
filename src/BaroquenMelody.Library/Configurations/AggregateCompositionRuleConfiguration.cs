using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Rules.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Configurations;

public sealed record AggregateCompositionRuleConfiguration(ISet<CompositionRuleConfiguration> Configurations)
{
    /// <summary>
    ///     Every rule is enabled by default, including <see cref="CompositionRule.EnforceVoiceSpacing"/>: the initial
    ///     voicing is validated against the rules, theme dead-ends are retried, and range configurations that can
    ///     never satisfy voice spacing have the rule disabled dynamically by the composer configurator.
    /// </summary>
    public static AggregateCompositionRuleConfiguration Default { get; } = new(
        EnumUtils<CompositionRule>
            .AsEnumerable()
            .Select(static compositionRule => new CompositionRuleConfiguration(compositionRule, ConfigurationStatus.Enabled))
            .ToFrozenSet()
    );
}
