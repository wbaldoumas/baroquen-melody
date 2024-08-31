using BaroquenMelody.Library.Compositions.Rules.Enums;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///     Represents a configuration for a composition rule.
/// </summary>
/// <param name="Rule">The composition rule type.</param>
/// <param name="IsEnabled">Whether the rule is enabled.</param>
/// <param name="Strictness">How strictly the rule should be enforced.</param>
public sealed record CompositionRuleConfiguration(
    CompositionRule Rule,
    bool IsEnabled = true,
    int Strictness = 100
);
