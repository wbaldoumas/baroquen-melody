using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Compositions.Rules.Enums;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRuleFactory"/>
internal sealed class CompositionRuleFactory(CompositionConfiguration compositionConfiguration, IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator) : ICompositionRuleFactory
{
    private const int Threshold = 100;

    public ICompositionRule CreateAggregate(AggregateCompositionRuleConfiguration aggregateConfiguration) => new AggregateCompositionRule(
        aggregateConfiguration.Configurations
            .Select(Create)
            .Prepend(new EnsureVoiceRange(compositionConfiguration))
            .ToList()
    );

    public ICompositionRule Create(CompositionRuleConfiguration configuration)
    {
        ICompositionRule compositionRule = configuration.Rule switch
        {
            ConfigurableCompositionRule.AvoidDissonance => new AvoidDissonance(),
            ConfigurableCompositionRule.AvoidDissonantLeaps => new AvoidDissonantLeaps(compositionConfiguration),
            ConfigurableCompositionRule.HandleAscendingSeventh => new HandleAscendingSeventh(compositionConfiguration),
            ConfigurableCompositionRule.AvoidRepetition => new AvoidRepetition(),
            ConfigurableCompositionRule.AvoidParallelFourths => new AvoidParallelIntervals(Interval.PerfectFourth),
            ConfigurableCompositionRule.AvoidParallelFifths => new AvoidParallelIntervals(Interval.PerfectFifth),
            ConfigurableCompositionRule.AvoidParallelOctaves => new AvoidParallelIntervals(Interval.Unison),
            ConfigurableCompositionRule.AvoidDirectFourths => new AvoidDirectIntervals(Interval.PerfectFourth),
            ConfigurableCompositionRule.AvoidDirectFifths => new AvoidDirectIntervals(Interval.PerfectFifth),
            ConfigurableCompositionRule.AvoidDirectOctaves => new AvoidDirectIntervals(Interval.Unison),
            ConfigurableCompositionRule.AvoidOverDoubling => new AvoidOverDoubling(),
            ConfigurableCompositionRule.FollowStandardChordProgression => new FollowsStandardProgression(compositionConfiguration),
            _ => throw new ArgumentOutOfRangeException(nameof(configuration), configuration.Rule, "The composition rule is not supported.")
        };

        return configuration.Strictness < Threshold ? new CompositionRuleBypass(compositionRule, weightedRandomBooleanGenerator, configuration.Strictness) : compositionRule;
    }
}
