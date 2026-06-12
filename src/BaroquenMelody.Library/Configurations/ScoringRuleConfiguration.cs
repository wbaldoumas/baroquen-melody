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
/// <param name="Weight">The weight applied to the rule's penalty when ranking candidate chords.</param>
public sealed record ScoringRuleConfiguration(ScoringRule Rule, ConfigurationStatus Status = ConfigurationStatus.Enabled, int Weight = 1)
{
    [JsonIgnore]
    public bool IsEnabled { get; } = Status.IsEnabled();

    [JsonIgnore]
    public bool IsFrozen { get; } = Status.IsFrozen();
}
