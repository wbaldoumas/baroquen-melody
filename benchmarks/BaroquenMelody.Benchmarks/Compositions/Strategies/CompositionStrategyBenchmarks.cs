using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Strategies;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.Logging;
using MusicalTimeSpan = Melanchall.DryWetMidi.Interaction.MusicalTimeSpan;
using Notes = Melanchall.DryWetMidi.MusicTheory.Notes;

namespace BaroquenMelody.Benchmarks.Compositions.Strategies;

/// <summary>
///     Compares candidate enumeration through the legacy Cartesian chord-choice repository (materialize every
///     combination, then filter) against the forward-checking enumerator (prune each voice's out-of-range choices
///     before enumerating). Mid-range anchors are the worst case for forward checking (nothing prunes, so the
///     comparison isolates its bookkeeping overhead); range-edge anchors show the pruning payoff that makes deeper
///     look-ahead affordable.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class CompositionStrategyBenchmarks
{
    private static readonly AggregateCompositionRule AggregateCompositionRule = new(
        [
            new EnsureInstrumentRange(BenchmarkData.CompositionConfiguration),
            new HandleAscendingSeventh(BenchmarkData.CompositionConfiguration),
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
    );

    private static readonly QuartetChordChoiceRepository ChordChoiceRepository = new(
        BenchmarkData.CompositionConfiguration,
        new NoteChoiceGenerator()
    );

    private static readonly CompositionStrategy ForwardCheckingCompositionStrategy = new(
        new ForwardCheckingChordChoiceEnumerator(
            BenchmarkData.CompositionConfiguration,
            new NoteChoiceGenerator()
        ),
        AggregateCompositionRule,
        LoggerFactory.Create(loggingBuilder => loggingBuilder.AddConsole()).CreateLogger("Benchmarks"),
        BenchmarkData.CompositionConfiguration,
        new ThreadLocalRandomProvider()
    );

    private static readonly IReadOnlyList<BaroquenChord> MidRangePrecedingChords = new FixedSizeList<BaroquenChord>(4)
    {
        BenchmarkData.CMajor, BenchmarkData.EMinor, BenchmarkData.FMajor, BenchmarkData.GMajor
    };

    /// <summary>
    ///     Ends on a chord voiced at the very top of every instrument's range, so every ascending choice is prunable.
    /// </summary>
    private static readonly IReadOnlyList<BaroquenChord> RangeEdgePrecedingChords = new FixedSizeList<BaroquenChord>(4)
    {
        BenchmarkData.CMajor,
        BenchmarkData.EMinor,
        BenchmarkData.FMajor,
        new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C6, MusicalTimeSpan.Quarter),
            new BaroquenNote(Instrument.Two, Notes.C6, MusicalTimeSpan.Quarter),
            new BaroquenNote(Instrument.Three, Notes.C6, MusicalTimeSpan.Quarter),
            new BaroquenNote(Instrument.Four, Notes.C6, MusicalTimeSpan.Quarter)
        ])
    };

    [Benchmark(Baseline = true)]
    public int LegacyRepositoryEnumerationMidRange() => EnumerateWithRepository(MidRangePrecedingChords);

    [Benchmark]
    public int ForwardCheckedEnumerationMidRange() =>
        ForwardCheckingCompositionStrategy.GetValidChordChoicesAndChords(MidRangePrecedingChords).Count();

    [Benchmark]
    public int LegacyRepositoryEnumerationRangeEdge() => EnumerateWithRepository(RangeEdgePrecedingChords);

    [Benchmark]
    public int ForwardCheckedEnumerationRangeEdge() =>
        ForwardCheckingCompositionStrategy.GetValidChordChoicesAndChords(RangeEdgePrecedingChords).Count();

    /// <summary>
    ///     The pre-forward-checking hot loop, verbatim: walk the whole Cartesian repository, materialize every
    ///     candidate, and let the rules reject the out-of-range ones.
    /// </summary>
    private static int EnumerateWithRepository(IReadOnlyList<BaroquenChord> precedingChords)
    {
        var currentChord = precedingChords[^1];
        var validCandidateCount = 0;

        for (var chordChoiceId = 0; chordChoiceId < ChordChoiceRepository.Count; ++chordChoiceId)
        {
            var chordChoice = ChordChoiceRepository.GetChordChoice(chordChoiceId);
            var nextChord = currentChord.ApplyChordChoice(BenchmarkData.CompositionConfiguration.Scale, chordChoice, BenchmarkData.CompositionConfiguration.DefaultNoteTimeSpan);

            if (AggregateCompositionRule.Evaluate(precedingChords, nextChord))
            {
                ++validCandidateCount;
            }
        }

        return validCandidateCount;
    }
}
