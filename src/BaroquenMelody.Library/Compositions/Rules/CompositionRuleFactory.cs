using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Compositions.Rules.Enums;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRuleFactory"/>
internal sealed class CompositionRuleFactory(
    CompositionConfiguration compositionConfiguration,
    IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator,
    IChordNumberIdentifier chordNumberIdentifier
) : ICompositionRuleFactory
{
    private const int Threshold = 100;

    public ICompositionRule CreateAggregate(AggregateCompositionRuleConfiguration aggregateConfiguration) => new AggregateCompositionRule(
        aggregateConfiguration.Configurations
            .Where(configuration => configuration.IsEnabled)
            .Select(Create)
            .Prepend(new EnsureVoiceRange(compositionConfiguration))
            .ToList()
    );

    public ICompositionRule Create(CompositionRuleConfiguration configuration)
    {
        ICompositionRule compositionRule = configuration.Rule switch
        {
            CompositionRule.AvoidDissonance => new AvoidDissonance(),
            CompositionRule.AvoidDissonantLeaps => new AvoidDissonantLeaps(compositionConfiguration),
            CompositionRule.HandleAscendingSeventh => new HandleAscendingSeventh(compositionConfiguration),
            CompositionRule.AvoidRepetition => new AvoidRepetition(),
            CompositionRule.AvoidParallelFourths => new AvoidParallelIntervals(Interval.PerfectFourth),
            CompositionRule.AvoidParallelFifths => new AvoidParallelIntervals(Interval.PerfectFifth),
            CompositionRule.AvoidParallelOctaves => new AvoidParallelIntervals(Interval.Unison),
            CompositionRule.AvoidDirectFourths => new AvoidDirectIntervals(Interval.PerfectFourth),
            CompositionRule.AvoidDirectFifths => new AvoidDirectIntervals(Interval.PerfectFifth),
            CompositionRule.AvoidDirectOctaves => new AvoidDirectIntervals(Interval.Unison),
            CompositionRule.AvoidOverDoubling => new AvoidOverDoubling(),
            CompositionRule.FollowStandardChordProgression => new FollowsStandardProgression(compositionConfiguration),
            CompositionRule.AvoidRepeatedChords => new AvoidRepeatedChords(chordNumberIdentifier),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration), configuration.Rule, "The composition rule is not supported.")
        };

        return configuration.Strictness < Threshold ? new CompositionRuleBypass(compositionRule, weightedRandomBooleanGenerator, configuration.Strictness) : compositionRule;
    }
}
