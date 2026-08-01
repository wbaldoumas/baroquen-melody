using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Rules.Harmonic;
using BaroquenMelody.Library.Rules.Melodic;
using BaroquenMelody.Library.Strategies;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Microsoft.Extensions.Logging;
using MusicalTimeSpan = Melanchall.DryWetMidi.Interaction.MusicalTimeSpan;
using Notes = Melanchall.DryWetMidi.MusicTheory.Notes;

namespace BaroquenMelody.Benchmarks.Compositions.Forms;

/// <summary>
///     Measures the ground bass form's hot path: every ground-note onset enumerates the rule-valid chords
///     carrying the pinned bass note, and threads each candidate to the next pin with a cheap existence check.
///     The planner runs once per composition and is included for completeness.
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class GroundBassBenchmarks
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

    private static readonly CompositionStrategy CompositionStrategy = new(
        new ForwardCheckingChordChoiceEnumerator(
            BenchmarkData.CompositionConfiguration,
            new NoteChoiceGenerator()
        ),
        AggregateCompositionRule,
        LoggerFactory.Create(static loggingBuilder => loggingBuilder.AddConsole()).CreateLogger("Benchmarks"),
        BenchmarkData.CompositionConfiguration,
        new ThreadLocalRandomProvider()
    );

    private static readonly IReadOnlyList<BaroquenChord> PrecedingChords = new FixedSizeList<BaroquenChord>(1)
    {
        BenchmarkData.DMinor
    };

    // The ground descending by step from the D Dorian tonic: the current onset pins C2, threading to B1.
    private static readonly BaroquenChord PinnedChord = new([new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)]);

    private static readonly BaroquenChord NextPinnedChord = new([new BaroquenNote(Instrument.Four, Notes.B1, MusicalTimeSpan.Half)]);

    [Benchmark]
    public int PinnedOnsetCandidates() => CompositionStrategy.GetRuleValidChordsForPartiallyVoicedChord(PrecedingChords, PinnedChord).Count;

    [Benchmark]
    public int PinnedOnsetCandidatesThreadedToTheNextPin() => CompositionStrategy
        .GetRuleValidChordsForPartiallyVoicedChord(PrecedingChords, PinnedChord)
        .Count(candidate => CompositionStrategy.HasPossibleChordForPartiallyVoicedChord([candidate], NextPinnedChord));

    [Benchmark]
    public int PlanGroundBass() => new GroundBassPlanner(BenchmarkData.CompositionConfiguration, new ThreadLocalRandomProvider()).CreatePlan()?.StatementCount ?? -1;
}
