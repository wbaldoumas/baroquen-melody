using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace BaroquenMelody.Benchmarks.Compositions.Rules;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class AggregateCompositionRuleBenchmarks
{
    private static readonly List<ICompositionRule> Rules =
    [
        new AvoidParallelIntervals(Interval.Unison),
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
        new AvoidParallelIntervals(Interval.PerfectFourth)
    ];

    private static readonly AggregateCompositionRule AggregateCompositionRule = new(Rules);

    private static readonly IReadOnlyList<BaroquenChord> PrecedingChords = new FixedSizeList<BaroquenChord>(4)
    {
        BenchmarkData.CMajor, BenchmarkData.EMinor, BenchmarkData.FMajor, BenchmarkData.GMajor
    };

    private static readonly BaroquenChord NextChord = BenchmarkData.DMinor;

    [Benchmark(Baseline = true)]
    public bool Evaluate()
    {
        var result = AggregateCompositionRule.Evaluate(PrecedingChords, NextChord);

        return result;
    }
}
