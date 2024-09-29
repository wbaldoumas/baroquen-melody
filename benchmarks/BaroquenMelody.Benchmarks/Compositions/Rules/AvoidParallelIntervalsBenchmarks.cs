using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace BaroquenMelody.Benchmarks.Compositions.Rules;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class AvoidParallelIntervalsBenchmarks
{
    private static readonly AvoidParallelIntervals AvoidParallelIntervals = new(Interval.PerfectFifth);

    private static readonly IReadOnlyList<BaroquenChord> PrecedingChords = new FixedSizeList<BaroquenChord>(4)
    {
        BenchmarkData.CMajor, BenchmarkData.EMinor, BenchmarkData.FMajor, BenchmarkData.CMajor
    };

    private static readonly BaroquenChord NextChord = BenchmarkData.DMinor;

    [Benchmark(Baseline = true)]
    public bool AvoidParallelIntervalsEvaluate()
    {
        var result = AvoidParallelIntervals.Evaluate(PrecedingChords, NextChord);

        return result;
    }
}
