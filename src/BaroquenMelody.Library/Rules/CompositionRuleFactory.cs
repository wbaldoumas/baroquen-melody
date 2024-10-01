using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules.Enums;

namespace BaroquenMelody.Library.Rules;

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
            .Prepend(new EnsureInstrumentRange(compositionConfiguration))
            .ToList()
    );

    public ICompositionRule Create(CompositionRuleConfiguration configuration)
    {
        ICompositionRule compositionRule = configuration.Rule switch
        {
            CompositionRule.AvoidDissonance => new AvoidDissonance(),
            CompositionRule.AvoidDissonantLeaps => new AvoidDissonantLeaps(compositionConfiguration),
            CompositionRule.HandleAscendingSeventh => new HandleAscendingSeventh(compositionConfiguration),
            CompositionRule.AvoidRepeatedNotes => new AvoidRepetition(),
            CompositionRule.AvoidParallelFourths => new AvoidParallelIntervals(Interval.PerfectFourth),
            CompositionRule.AvoidParallelFifths => new AvoidParallelIntervals(Interval.PerfectFifth),
            CompositionRule.AvoidParallelOctaves => new AvoidParallelIntervals(Interval.Unison),
            CompositionRule.AvoidDirectFourths => new AvoidDirectIntervals(Interval.PerfectFourth, compositionConfiguration),
            CompositionRule.AvoidDirectFifths => new AvoidDirectIntervals(Interval.PerfectFifth, compositionConfiguration),
            CompositionRule.AvoidDirectOctaves => new AvoidDirectIntervals(Interval.Unison, compositionConfiguration),
            CompositionRule.AvoidOverDoubling => new AvoidOverDoubling(),
            CompositionRule.FollowStandardChordProgression => new FollowsStandardProgression(compositionConfiguration),
            CompositionRule.AvoidRepeatedChords => new AvoidRepeatedChords(chordNumberIdentifier),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration), configuration.Rule, "The composition rule is not supported.")
        };

        return configuration.Strictness < Threshold ? new CompositionRuleBypass(compositionRule, weightedRandomBooleanGenerator, configuration.Strictness) : compositionRule;
    }
}
