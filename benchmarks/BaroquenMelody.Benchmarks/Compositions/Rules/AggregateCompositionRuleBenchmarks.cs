using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Infrastructure.Collections;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace BaroquenMelody.Benchmarks.Compositions.Rules;

[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]
public class AggregateCompositionRuleBenchmarks
{
    private static readonly AggregateCompositionRule AggregateCompositionRule = new(
        [
            new HandleAscendingSeventh(BenchmarkData.CompositionConfiguration),
            new EnsureInstrumentRange(BenchmarkData.CompositionConfiguration),
            new AvoidDissonance(),
            new AvoidDissonantLeaps(BenchmarkData.CompositionConfiguration),
            new AvoidRepetition(),
            new AvoidParallelIntervals(Interval.PerfectFifth),
            new AvoidParallelIntervals(Interval.PerfectFourth),
            new AvoidParallelIntervals(Interval.Unison)
        ]
    );

    private static readonly IReadOnlyList<BaroquenChord> PrecedingChords = new FixedSizeList<BaroquenChord>(4)
    {
        BenchmarkData.CMajor, BenchmarkData.EMinor, BenchmarkData.FMajor, BenchmarkData.GMajor
    };

    private static readonly BaroquenChord NextChord = BenchmarkData.DMinor;

    [Benchmark]
    public bool Evaluate()
    {
        var result = AggregateCompositionRule.Evaluate(PrecedingChords, NextChord);

        return result;
    }
}
