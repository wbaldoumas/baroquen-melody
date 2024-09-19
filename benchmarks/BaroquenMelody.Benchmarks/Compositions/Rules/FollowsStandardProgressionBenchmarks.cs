using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Infrastructure.Collections;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace BaroquenMelody.Benchmarks.Compositions.Rules;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class FollowsStandardProgressionBenchmarks
{
    private static readonly FollowsStandardProgression FollowsStandardProgression = new(BenchmarkData.CompositionConfiguration);

    private static readonly IReadOnlyList<BaroquenChord> PrecedingChords = new FixedSizeList<BaroquenChord>(4)
    {
        BenchmarkData.CMajor, BenchmarkData.EMinor, BenchmarkData.FMajor, BenchmarkData.GMajor
    };

    private static readonly BaroquenChord NextChord = BenchmarkData.DMinor;

    [Benchmark(Baseline = true)]
    public bool FollowsStandardProgressionEvaluate()
    {
        var result = FollowsStandardProgression.Evaluate(PrecedingChords, NextChord);

        return result;
    }
}
