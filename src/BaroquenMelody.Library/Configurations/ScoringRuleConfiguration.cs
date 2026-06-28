using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Enums.Extensions;
using BaroquenMelody.Library.Scoring.Enums;
using System.Text.Json.Serialization;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     Represents a configuration for a scoring rule.
/// </summary>
/// <param name="Rule">The scoring rule type.</param>
/// <param name="Status">Whether the rule is enabled, locked, or disabled.</param>
/// <param name="Weight">
///     Multiplies this rule's penalty before the per-rule penalties are summed for chord selection. A higher weight
///     makes the preference stricter, so it dominates when rules conflict; a lower weight (toward 1) makes it a gentle
///     nudge that other rules can easily override; a weight of 0 (or a disabled <paramref name="Status"/>) removes the
///     rule from scoring entirely.
/// </param>
public sealed record ScoringRuleConfiguration(ScoringRule Rule, ConfigurationStatus Status = ConfigurationStatus.Enabled, int Weight = 1)
{
    [JsonIgnore]
    public bool IsEnabled { get; } = Status.IsEnabled();

    [JsonIgnore]
    public bool IsFrozen { get; } = Status.IsFrozen();
}
