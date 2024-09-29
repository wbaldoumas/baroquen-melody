using BaroquenMelody.Library.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Melanchall.DryWetMidi.Interaction;

namespace BaroquenMelody.Benchmarks.Compositions.Extensions;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class BaroquenChordExtensionsBenchmarks
{
    [Benchmark(Baseline = true)]
    public int ApplyChordChoice()
    {
        var baroquenChord = BenchmarkData.CMajor.ApplyChordChoice(
            BenchmarkData.BaroquenScale,
            BenchmarkData.ChordChoice,
            MusicalTimeSpan.Quarter
        );

        return baroquenChord.Notes.Count;
    }
}
