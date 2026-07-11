using BaroquenMelody.Benchmarks.Compositions.Strategies;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Rules.Melodic;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Strategies;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Benchmarks.Compositions.Scoring;

/// <summary>
///     Compares the cost of the legacy uniform random chord pick against the <see cref="WeightedChordSelector"/>
///     with scoring disabled (the behavior-compatible path) and with the default scoring rules enabled, over the
///     same rule-passing candidate set. <see cref="CompositionStrategyBenchmarks"/>-style enumeration is included
///     for context, since candidate enumeration, not selection, dominates the composition hot path.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class WeightedChordSelectorBenchmarks
{
    private static readonly CompositionStrategy CompositionStrategy = new(
        new ForwardCheckingChordChoiceEnumerator(
            BenchmarkData.CompositionConfiguration,
            new NoteChoiceGenerator()
        ),
        new AggregateCompositionRule(
            [
                new MelodicCompositionRuleAdapter(new HandleAscendingSeventh(BenchmarkData.CompositionConfiguration)),
                new EnsureInstrumentRange(BenchmarkData.CompositionConfiguration),
                new AvoidDirectIntervals(Interval.PerfectFifth, BenchmarkData.CompositionConfiguration),
                new AvoidDirectIntervals(Interval.PerfectFourth, BenchmarkData.CompositionConfiguration),
                new AvoidDirectIntervals(Interval.Unison, BenchmarkData.CompositionConfiguration),
                new AvoidOverDoubling(),
                new AvoidRepeatedChords(new ChordNumberIdentifier(BenchmarkData.CompositionConfiguration)),
                new FollowsStandardProgression(BenchmarkData.CompositionConfiguration),
                new AvoidDissonance(),
                new MelodicCompositionRuleAdapter(new AvoidDissonantLeaps(BenchmarkData.CompositionConfiguration)),
                new AvoidRepetition(),
                new AvoidParallelIntervals(Interval.PerfectFifth),
                new AvoidParallelIntervals(Interval.PerfectFourth),
                new AvoidParallelIntervals(Interval.Unison)
            ]
        ),
        LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole()).CreateLogger("Benchmarks"),
        BenchmarkData.CompositionConfiguration,
        new ThreadLocalRandomProvider()
    );

    private static readonly IReadOnlyList<BaroquenChord> PrecedingChords = new FixedSizeList<BaroquenChord>(4)
    {
        BenchmarkData.CMajor, BenchmarkData.EMinor, BenchmarkData.FMajor, BenchmarkData.GMajor
    };

    private static readonly IReadOnlyList<BaroquenChord> CandidateChords = CompositionStrategy
        .GetValidChordChoicesAndChords(PrecedingChords)
        .Select(static chordChoiceAndChord => chordChoiceAndChord.Chord)
        .ToList();

    private static readonly IRandomProvider RandomProvider = new ThreadLocalRandomProvider();

    private static readonly WeightedChordSelector ScoringDisabledChordSelector = new(
        new AggregateScoringRule([]),
        RandomProvider
    );

    private static readonly WeightedChordSelector DefaultScoringChordSelector = new(
        new ScoringRuleFactory(BenchmarkData.CompositionConfiguration).CreateAggregate(AggregateScoringRuleConfiguration
            .Default),
        RandomProvider
    );

    [Benchmark(Baseline = true)]
    public int LegacyUniformRandomPick() => CandidateChords.MinByRandom(RandomProvider)!.Notes[0].NoteNumber;

    [Benchmark]
    public int WeightedChordSelectorWithScoringDisabled() =>
        ScoringDisabledChordSelector.SelectNextChord(PrecedingChords, CandidateChords)!.Notes[0].NoteNumber;

    [Benchmark]
    public int WeightedChordSelectorWithDefaultScoring() =>
        DefaultScoringChordSelector.SelectNextChord(PrecedingChords, CandidateChords)!.Notes[0].NoteNumber;

    [Benchmark]
    public int EnumerateValidChordChoicesAndChords() =>
        CompositionStrategy.GetValidChordChoicesAndChords(PrecedingChords).Count();
}
