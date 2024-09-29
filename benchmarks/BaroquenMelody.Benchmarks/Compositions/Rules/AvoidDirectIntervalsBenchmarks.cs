using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace BaroquenMelody.Benchmarks.Compositions.Rules;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class AvoidDirectIntervalsBenchmarks
{
    private static readonly AvoidDirectIntervals AvoidDirectIntervals = new(
        Interval.PerfectFifth,
        BenchmarkData.CompositionConfiguration
    );

    private static readonly IReadOnlyList<BaroquenChord> PrecedingChords = new FixedSizeList<BaroquenChord>(4)
    {
        BenchmarkData.CMajor, BenchmarkData.EMinor, BenchmarkData.FMajor, BenchmarkData.GMajor
    };

    private static readonly BaroquenChord NextChord = BenchmarkData.DMinor;

    [Benchmark(Baseline = true)]
    public bool AvoidDirectIntervalsEvaluate()
    {
        var result = AvoidDirectIntervals.Evaluate(PrecedingChords, NextChord);

        return result;
    }
}
