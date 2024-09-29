using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Extensions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;

namespace BaroquenMelody.Benchmarks.Compositions.Extensions;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class BaroquenNoteExtensionsBenchmarks
{
    private static readonly BaroquenNote Note = new(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter);

    private readonly NoteChoice? _noteChoice = new(Instrument.One, NoteMotion.Ascending, 3);

    [Benchmark(Baseline = true)]
    public BaroquenNote ApplyNoteChoice()
    {
        var baroquenNote = Note.ApplyNoteChoice(BenchmarkData.BaroquenScale, _noteChoice!, MusicalTimeSpan.Quarter);

        return baroquenNote;
    }
}
