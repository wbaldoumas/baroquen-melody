using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Enums.Extensions;
using BaroquenMelody.Library.Rules.Enums;
using System.Text.Json.Serialization;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     Represents a configuration for a composition rule.
/// </summary>
/// <param name="Rule">The composition rule type.</param>
/// <param name="Status">Whether the rule is enabled, locked, or disabled.</param>
/// <param name="Strictness">How strictly the rule should be enforced.</param>
public sealed record CompositionRuleConfiguration(CompositionRule Rule, ConfigurationStatus Status = ConfigurationStatus.Enabled, int Strictness = 100)
{
    // computed from the status rather than captured at construction time, so with-expressions that
    // change the status (which clone the record without re-running the constructor) stay consistent.
    [JsonIgnore]
    public bool IsEnabled => Status.IsEnabled();

    [JsonIgnore]
    public bool IsFrozen => Status.IsFrozen();
}
