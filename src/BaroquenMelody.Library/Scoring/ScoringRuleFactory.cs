using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Scoring.Enums;
using BaroquenMelody.Library.Scoring.Harmonic;
using BaroquenMelody.Library.Scoring.Melodic;

namespace BaroquenMelody.Library.Scoring;

/// <inheritdoc cref="IScoringRuleFactory"/>
internal sealed class ScoringRuleFactory(
    CompositionConfiguration compositionConfiguration,
    IChordNumberIdentifier chordNumberIdentifier,
    IChordInversionIdentifier chordInversionIdentifier
) : IScoringRuleFactory
{
    // Rule order is load-bearing: the aggregate sums each rule's weighted penalty in this order, and floating-point
    // addition is order-sensitive, so it feeds the softmax chord selection. Ordering by the enum keeps it a pure
    // function of the configuration rather than of whatever order the caller's set happens to enumerate in.
    public IScoringRule CreateAggregate(AggregateScoringRuleConfiguration aggregateConfiguration) => new AggregateScoringRule(
        aggregateConfiguration.Configurations
            .Where(static configuration => configuration.IsEnabled && configuration.Weight > 0)
            .OrderBy(static configuration => configuration.Rule)
            .Select(Create)
            .ToList()
    );

    public IScoringRule Create(ScoringRuleConfiguration configuration)
    {
        IScoringRule scoringRule = configuration.Rule switch
        {
            ScoringRule.PreferShortestVoiceMovement => new MelodicScoringRuleAdapter(new PreferShortestVoiceMovement(compositionConfiguration)),
            ScoringRule.PreferContraryOuterVoiceMotion => new PreferContraryOuterVoiceMotion(compositionConfiguration),
            ScoringRule.PreferLeapRecovery => new MelodicScoringRuleAdapter(new PreferLeapRecovery(compositionConfiguration)),
            ScoringRule.PreferStableChordInversions => new PreferStableChordInversions(chordInversionIdentifier),
            ScoringRule.PreferHarmonicSequences => new PreferHarmonicSequences(chordNumberIdentifier),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration), configuration.Rule, "The scoring rule is not supported.")
        };

        return new WeightedScoringRule(scoringRule, configuration.Weight);
    }
}
