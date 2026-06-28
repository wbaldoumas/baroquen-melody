using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Scoring.Enums;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Configurations;

[ExcludeFromCodeCoverage(Justification = "Configuration")]
public sealed record AggregateScoringRuleConfiguration(ISet<ScoringRuleConfiguration> Configurations)
{
    // Each rule's Weight multiplies its penalty (0 = ideal) before the per-rule penalties are summed and fed to the
    // softmax chord selector, which prefers candidates with the lower total. A higher weight makes that preference
    // bite harder: the composer follows it more strictly and it wins when two preferences conflict. A lower weight
    // (toward 1) makes it a gentle nudge that other rules or the selector's randomness can override; weight 0 (or a
    // disabled rule) drops it entirely. Only the relative sizes decide conflicts, while the overall magnitude trades
    // off against the selector's Temperature (scaling all weights up sharpens selection toward the smoothest option;
    // scaling them down flattens it toward a random pick).
    public static AggregateScoringRuleConfiguration Default { get; } = new(
        new HashSet<ScoringRuleConfiguration>
        {
            new(ScoringRule.PreferShortestVoiceMovement, ConfigurationStatus.Enabled, Weight: 2), // lightest: smooths lines without freezing voice motion
            new(ScoringRule.PreferContraryOuterVoiceMotion, ConfigurationStatus.Enabled, Weight: 4), // stronger: outer-voice independence beats raw smoothness
            new(ScoringRule.PreferLeapRecovery, ConfigurationStatus.Enabled, Weight: 4) // stronger: leaps still resolve, even if recovery moves more
        }
    );
}
