using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Rules.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Configurations;

public sealed record AggregateCompositionRuleConfiguration(ISet<CompositionRuleConfiguration> Configurations)
{
    /// <summary>
    ///     Rules that are available but switched off by default. <see cref="CompositionRule.EnforceVoiceSpacing"/>
    ///     over-constrains the current greedy look-ahead search and can leave the composer without a valid next chord,
    ///     so it is opt-in until forward checking makes a deeper constrained search affordable.
    /// </summary>
    private static readonly FrozenSet<CompositionRule> _disabledByDefault = new[]
    {
        CompositionRule.EnforceVoiceSpacing
    }.ToFrozenSet();

    public static AggregateCompositionRuleConfiguration Default { get; } = new(
        EnumUtils<CompositionRule>
            .AsEnumerable()
            .Select(static compositionRule => new CompositionRuleConfiguration(
                compositionRule,
                _disabledByDefault.Contains(compositionRule) ? ConfigurationStatus.Disabled : ConfigurationStatus.Enabled))
            .ToFrozenSet()
    );
}
