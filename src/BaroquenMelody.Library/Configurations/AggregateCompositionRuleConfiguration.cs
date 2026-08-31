using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Rules.Enums;

namespace BaroquenMelody.Library.Configurations;

public sealed record AggregateCompositionRuleConfiguration(ISet<CompositionRuleConfiguration> Configurations)
{
    /// <summary>
    ///     Every rule is enabled by default, including <see cref="CompositionRule.EnforceVoiceSpacing"/>: the initial
    ///     voicing is validated against the rules, theme dead-ends are retried, and range configurations that can
    ///     never satisfy voice spacing have the rule disabled dynamically by the composer configurator.
    /// </summary>
    /// <remarks>
    ///     An insertion-ordered <see cref="HashSet{T}"/>, never a frozen set: a frozen set of records enumerates in
    ///     hash-bucket order, which folds in the identity hash of the record's type and so varies with process history.
    /// </remarks>
    public static AggregateCompositionRuleConfiguration Default { get; } = new(
        EnumUtils<CompositionRule>
            .AsEnumerable()
            .Select(static compositionRule => new CompositionRuleConfiguration(compositionRule, ConfigurationStatus.Enabled))
            .ToHashSet()
    );
}
