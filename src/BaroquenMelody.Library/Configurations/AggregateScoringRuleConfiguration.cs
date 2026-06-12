using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Scoring.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record AggregateScoringRuleConfiguration(ISet<ScoringRuleConfiguration> Configurations)
{
    public static AggregateScoringRuleConfiguration Default { get; } = new(
        new HashSet<ScoringRuleConfiguration>
        {
            new(ScoringRule.PreferShortestVoiceMovement, ConfigurationStatus.Enabled, Weight: 2),
            new(ScoringRule.PreferContraryOuterVoiceMotion, ConfigurationStatus.Enabled, Weight: 4),
            new(ScoringRule.PreferLeapRecovery, ConfigurationStatus.Enabled, Weight: 4)
        }
    );
}
