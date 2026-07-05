using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Rules.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Configurations;

public sealed record AggregateCompositionRuleConfiguration(ISet<CompositionRuleConfiguration> Configurations)
{
    /// <summary>
    ///     Rules that are available but switched off by default. <see cref="CompositionRule.EnforceVoiceSpacing"/> now
    ///     composes at no measurable cost (forward checking prunes the search), but it must stay opt-in: the initial
    ///     and fugal-entry voicings are not rule-checked, so a composition can start from a spacing-unsatisfiable
    ///     position that no subsequent chord can repair, dead-ending the composer. Enabling it by default is blocked
    ///     on validating those starting voicings.
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
