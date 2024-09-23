using BaroquenMelody.Library.Compositions.Rules.Enums;
using BaroquenMelody.Library.Infrastructure.Configuration.Enums;
using BaroquenMelody.Library.Infrastructure.Configuration.Enums.Extensions;
using System.Text.Json.Serialization;

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
///     Represents a configuration for a composition rule.
/// </summary>
/// <param name="Rule">The composition rule type.</param>
/// <param name="Status">Whether the rule is enabled, locked, or disabled.</param>
/// <param name="Strictness">How strictly the rule should be enforced.</param>
public sealed record CompositionRuleConfiguration(CompositionRule Rule, ConfigurationStatus Status = ConfigurationStatus.Enabled, int Strictness = 100)
{
    [JsonIgnore]
    public bool IsEnabled { get; } = Status.IsEnabled();

    [JsonIgnore]
    public bool IsFrozen { get; } = Status.IsFrozen();
}
