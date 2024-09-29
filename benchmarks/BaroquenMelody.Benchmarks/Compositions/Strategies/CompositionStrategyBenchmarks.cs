using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Strategies;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.Logging;

namespace BaroquenMelody.Benchmarks.Compositions.Strategies;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CompositionStrategyBenchmarks
{
    private static readonly CompositionStrategy CompositionStrategy = new(
        new QuartetChordChoiceRepository(
            BenchmarkData.CompositionConfiguration,
            new NoteChoiceGenerator()
        ),
        new AggregateCompositionRule(
            [
                new HandleAscendingSeventh(BenchmarkData.CompositionConfiguration),
                new EnsureInstrumentRange(BenchmarkData.CompositionConfiguration),
                new AvoidDirectIntervals(Interval.PerfectFifth, BenchmarkData.CompositionConfiguration),
                new AvoidDirectIntervals(Interval.PerfectFourth, BenchmarkData.CompositionConfiguration),
                new AvoidDirectIntervals(Interval.Unison, BenchmarkData.CompositionConfiguration),
                new AvoidOverDoubling(),
                new AvoidRepeatedChords(new ChordNumberIdentifier(BenchmarkData.CompositionConfiguration)),
                new FollowsStandardProgression(BenchmarkData.CompositionConfiguration),
                new AvoidDissonance(),
                new AvoidDissonantLeaps(BenchmarkData.CompositionConfiguration),
                new AvoidRepetition(),
                new AvoidParallelIntervals(Interval.PerfectFifth),
                new AvoidParallelIntervals(Interval.PerfectFourth),
                new AvoidParallelIntervals(Interval.Unison)
            ]
        ),
        LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole()).CreateLogger("Benchmarks"),
        BenchmarkData.CompositionConfiguration
    );

    private static readonly IReadOnlyList<BaroquenChord> PrecedingChords = new FixedSizeList<BaroquenChord>(4)
    {
        BenchmarkData.CMajor, BenchmarkData.EMinor, BenchmarkData.FMajor, BenchmarkData.GMajor
    };

    [Benchmark(Baseline = true)]
    public int CompositionStrategyGetValidChordChoicesAndChords()
    {
        var result = CompositionStrategy.GetValidChordChoicesAndChords(PrecedingChords);

        return result.Count();
    }
}
