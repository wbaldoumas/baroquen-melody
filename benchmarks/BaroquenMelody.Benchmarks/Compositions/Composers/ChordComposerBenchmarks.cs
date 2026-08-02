using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Rules.Harmonic;
using BaroquenMelody.Library.Rules.Melodic;
using BaroquenMelody.Library.Scoring;
using BaroquenMelody.Library.Strategies;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Benchmarks.Compositions.Composers;

/// <summary>
///     Measures the body walk's per-beat emit path against its held-voice variant: the pinned overload
///     enumerates the same look-ahead-vetted candidate set once and filters it, so its cost should sit at the
///     free path's cost plus a linear scan - never a second enumeration.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class ChordComposerBenchmarks
{
    private static readonly AggregateCompositionRule AggregateCompositionRule = new(
        [
            new EnsureInstrumentRange(BenchmarkData.CompositionConfiguration),
            new MelodicCompositionRuleAdapter(new HandleAscendingSeventh(BenchmarkData.CompositionConfiguration)),
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
    );

    private static readonly ChordComposer ChordComposer = new(
        new CompositionStrategy(
            new ForwardCheckingChordChoiceEnumerator(
                BenchmarkData.CompositionConfiguration,
                new NoteChoiceGenerator()
            ),
            AggregateCompositionRule,
            LoggerFactory.Create(static loggingBuilder => loggingBuilder.AddConsole()).CreateLogger("Benchmarks"),
            BenchmarkData.CompositionConfiguration,
            new ThreadLocalRandomProvider()
        ),
        new WeightedChordSelector(
            new ScoringRuleFactory(
                BenchmarkData.CompositionConfiguration,
                new ChordNumberIdentifier(BenchmarkData.CompositionConfiguration),
                new ChordInversionIdentifier(new ChordNumberIdentifier(BenchmarkData.CompositionConfiguration), BenchmarkData.CompositionConfiguration)
            ).CreateAggregate(Library.Configurations.AggregateScoringRuleConfiguration.Default),
            new ThreadLocalRandomProvider()
        ),
        LoggerFactory.Create(static loggingBuilder => loggingBuilder.AddConsole()).CreateLogger("Benchmarks")
    );

    private static readonly IReadOnlyList<BaroquenChord> PrecedingChords = new FixedSizeList<BaroquenChord>(1)
    {
        BenchmarkData.DMinor
    };

    [Benchmark]
    public int ComposeFree() => ChordComposer.Compose(PrecedingChords).Notes.Count;

    [Benchmark]
    public int ComposeWithPin() => ChordComposer.Compose(PrecedingChords, BenchmarkData.DMinor[Instrument.One]).Notes.Count;
}
