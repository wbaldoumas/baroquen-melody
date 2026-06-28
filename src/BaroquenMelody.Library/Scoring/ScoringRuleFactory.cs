using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Scoring.Enums;

namespace BaroquenMelody.Library.Scoring;

/// <inheritdoc cref="IScoringRuleFactory"/>
internal sealed class ScoringRuleFactory(CompositionConfiguration compositionConfiguration) : IScoringRuleFactory
{
    public IScoringRule CreateAggregate(AggregateScoringRuleConfiguration aggregateConfiguration) => new AggregateScoringRule(
        aggregateConfiguration.Configurations
            .Where(static configuration => configuration.IsEnabled && configuration.Weight > 0)
            .Select(Create)
            .ToList()
    );

    public IScoringRule Create(ScoringRuleConfiguration configuration)
    {
        IScoringRule scoringRule = configuration.Rule switch
        {
            ScoringRule.PreferShortestVoiceMovement => new PreferShortestVoiceMovement(compositionConfiguration),
            ScoringRule.PreferContraryOuterVoiceMotion => new PreferContraryOuterVoiceMotion(compositionConfiguration),
            ScoringRule.PreferLeapRecovery => new PreferLeapRecovery(compositionConfiguration),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration), configuration.Rule, "The scoring rule is not supported.")
        };

        return new WeightedScoringRule(scoringRule, configuration.Weight);
    }
}
