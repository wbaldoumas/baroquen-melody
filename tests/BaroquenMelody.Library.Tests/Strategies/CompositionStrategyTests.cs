using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Exceptions;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Strategies;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Strategies;

[TestFixture]
internal sealed class CompositionStrategyTests
{
    private IChordChoiceEnumerator _mockChordChoiceEnumerator = default!;

    private ICompositionRule _mockCompositionRule = default!;

    private ILogger _mockLogger = default!;

    private CompositionStrategy _compositionStrategy = default!;

    private CompositionConfiguration _compositionConfiguration = default!;

    [SetUp]
    public void Setup()
    {
        _mockChordChoiceEnumerator = Substitute.For<IChordChoiceEnumerator>();
        _mockCompositionRule = Substitute.For<ICompositionRule>();
        _mockLogger = Substitute.For<ILogger>();

        _compositionConfiguration = TestCompositionConfigurations.Get();

        _compositionStrategy = new CompositionStrategy(
            _mockChordChoiceEnumerator,
            _mockCompositionRule,
            _mockLogger,
            _compositionConfiguration,
            new ThreadLocalRandomProvider()
        );
    }

    [Test]
    public void GetInitialChord_generates_valid_chord()
    {
        // arrange - the rule accepts every candidate, so the first generated voicing is returned
        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(true);

        // run this test a bunch to account for randomness
        for (var i = 0; i < 1000; ++i)
        {
            // act
            var chord = _compositionStrategy.GenerateInitialChord();

            // assert
            chord.Notes.Should().HaveCount(4);

            // since there are four instruments but only three unique notes in a C Major chord, there should be at least one instrument with a repeated note
            chord.Notes.GroupBy(note => note.NoteName).Should().HaveCount(3).And.OnlyContain(grouping => grouping.Count() <= 2);

            foreach (var baroquenNote in chord.Notes)
            {
                baroquenNote.NoteName.Should().BeOneOf(
                    NoteName.C,
                    NoteName.E,
                    NoteName.G
                );
            }
        }
    }

    [Test]
    public void GetPossibleChordChoices_returns_expected_chord_choices()
    {
        // arrange
        var precedingChords = new List<BaroquenChord>
        {
            new([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
            ])
        };

        var goodChordChoice = new ChordChoice([
            new NoteChoice(Instrument.One, NoteMotion.Oblique, 0),
            new NoteChoice(Instrument.Two, NoteMotion.Oblique, 0),
            new NoteChoice(Instrument.Three, NoteMotion.Oblique, 0),
            new NoteChoice(Instrument.Four, NoteMotion.Oblique, 0)
        ]);

        var otherGoodChordChoice = new ChordChoice([
            new NoteChoice(Instrument.One, NoteMotion.Ascending, 1),
            new NoteChoice(Instrument.Two, NoteMotion.Descending, 1),
            new NoteChoice(Instrument.Three, NoteMotion.Ascending, 1),
            new NoteChoice(Instrument.Four, NoteMotion.Descending, 1)
        ]);

        var badChordChoice = new ChordChoice([
            new NoteChoice(Instrument.One, NoteMotion.Ascending, 5),
            new NoteChoice(Instrument.Two, NoteMotion.Descending, 5),
            new NoteChoice(Instrument.Three, NoteMotion.Ascending, 5),
            new NoteChoice(Instrument.Four, NoteMotion.Descending, 5)
        ]);

        var mockChordChoices = new List<ChordChoice>
        {
            goodChordChoice,
            badChordChoice,
            otherGoodChordChoice
        };

        // The enumerator yields the same materialized candidates for every enumeration, including the recursive
        // look-ahead passes.
        var candidates = mockChordChoices
            .Select(chordChoice => (ChordChoice: chordChoice, Chord: precedingChords[^1].ApplyChordChoice(_compositionConfiguration.Scale, chordChoice, _compositionConfiguration.DefaultNoteTimeSpan)))
            .ToList();

        _mockChordChoiceEnumerator.EnumerateCandidates(Arg.Any<BaroquenChord>()).Returns(candidates);

        const bool goodChordChoiceResult = true;
        const bool otherGoodChordChoiceResult = true;
        const bool badChordChoiceResult = false;

        var mockChordChoiceEvaluationResults = new List<bool>
        {
            goodChordChoiceResult,
            badChordChoiceResult,
            otherGoodChordChoiceResult
        };

        var mockCompositionRuleReturns = new List<bool>();

        foreach (var outerChordChoiceEvaluationResult in mockChordChoiceEvaluationResults)
        {
            mockCompositionRuleReturns.Add(outerChordChoiceEvaluationResult);

            foreach (var innerChordChoiceEvaluationResult in mockChordChoiceEvaluationResults)
            {
                mockCompositionRuleReturns.Add(innerChordChoiceEvaluationResult);
                mockCompositionRuleReturns.AddRange(mockChordChoiceEvaluationResults);
            }
        }

        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(
            mockCompositionRuleReturns[0],
            mockCompositionRuleReturns.ToArray()[1..]
        );

        // act
        var possibleChordChoices = _compositionStrategy.GetPossibleChordChoices(precedingChords).ToList();

        // Assert
        possibleChordChoices.Should().NotBeNull();
    }

    [Test]
    public void GetPossibleChordsForPartiallyVoicedChords_returns_expected_chords()
    {
        // arrange
        var precedingChords = new List<BaroquenChord>
        {
            new([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
            ])
        };

        var nextChord = new BaroquenChord([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
            ]
        );

        var goodChordChoice = new ChordChoice([
            new NoteChoice(Instrument.One, NoteMotion.Oblique, 0),
            new NoteChoice(Instrument.Two, NoteMotion.Oblique, 0),
            new NoteChoice(Instrument.Three, NoteMotion.Oblique, 0),
            new NoteChoice(Instrument.Four, NoteMotion.Oblique, 0)
        ]);

        var otherGoodChordChoice = new ChordChoice([
            new NoteChoice(Instrument.One, NoteMotion.Ascending, 1),
            new NoteChoice(Instrument.Two, NoteMotion.Descending, 1),
            new NoteChoice(Instrument.Three, NoteMotion.Ascending, 1),
            new NoteChoice(Instrument.Four, NoteMotion.Descending, 1)
        ]);

        var badChordChoice = new ChordChoice([
            new NoteChoice(Instrument.One, NoteMotion.Ascending, 5),
            new NoteChoice(Instrument.Two, NoteMotion.Descending, 5),
            new NoteChoice(Instrument.Three, NoteMotion.Ascending, 5),
            new NoteChoice(Instrument.Four, NoteMotion.Descending, 5)
        ]);

        var mockChordChoices = new List<ChordChoice>
        {
            goodChordChoice,
            badChordChoice,
            otherGoodChordChoice
        };

        // The enumerator yields the same materialized candidates for every enumeration, including the recursive
        // look-ahead passes.
        var candidates = mockChordChoices
            .Select(chordChoice => (ChordChoice: chordChoice, Chord: precedingChords[^1].ApplyChordChoice(_compositionConfiguration.Scale, chordChoice, _compositionConfiguration.DefaultNoteTimeSpan)))
            .ToList();

        _mockChordChoiceEnumerator.EnumerateCandidates(Arg.Any<BaroquenChord>()).Returns(candidates);

        const bool goodChordChoiceResult = true;

        const bool otherGoodChordChoiceResult = true;

        const bool badChordChoiceResult = false;

        var mockChordChoiceEvaluationResults = new List<bool>
        {
            goodChordChoiceResult,
            badChordChoiceResult,
            otherGoodChordChoiceResult
        };

        var mockCompositionRuleReturns = new List<bool>();

        foreach (var outerChordChoiceEvaluationResult in mockChordChoiceEvaluationResults)
        {
            mockCompositionRuleReturns.Add(outerChordChoiceEvaluationResult);

            foreach (var innerChordChoiceEvaluationResult in mockChordChoiceEvaluationResults)
            {
                mockCompositionRuleReturns.Add(innerChordChoiceEvaluationResult);
                mockCompositionRuleReturns.AddRange(mockChordChoiceEvaluationResults);
            }
        }

        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(
            mockCompositionRuleReturns[0],
            mockCompositionRuleReturns.ToArray()[1..]
        );

        // act
        var possibleChords = _compositionStrategy.GetPossibleChordsForPartiallyVoicedChords(precedingChords, nextChord).ToList();

        // Assert
        possibleChords.Should().NotBeNull();
    }

    [Test]
    public void GetRuleValidChordsForPartiallyVoicedChord_ReturnsRuleValidChordsContainingThePinnedNotes_WithoutLookAhead()
    {
        // arrange - three candidates: one keeps instrument One on C4 (matching the pin) and passes the rule,
        // one matches nothing, and one passes the rule but moves One off the pinned note
        var (precedingChords, pinnedChord) = ArrangePartiallyVoicedCandidates();

        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(true, false, true);

        // act
        var ruleValidChords = _compositionStrategy.GetRuleValidChordsForPartiallyVoicedChord(precedingChords, pinnedChord);

        // assert - only the rule-valid candidate containing the pinned note survives, and each candidate was
        // evaluated exactly once: pinned beats must not run the free-choice look-ahead, which would starve a
        // beat whose successor is already dictated
        ruleValidChords.Should().ContainSingle().Which[Instrument.One].Raw.Should().Be(Notes.C4);
        _mockCompositionRule.Received(3).Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>());
    }

    [Test]
    public void HasPossibleChordForPartiallyVoicedChord_WhenARuleValidChordContainsThePinnedNotes_ReturnsTrue()
    {
        // arrange
        var (precedingChords, pinnedChord) = ArrangePartiallyVoicedCandidates();

        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(true);

        // act
        var hasPossibleChord = _compositionStrategy.HasPossibleChordForPartiallyVoicedChord(precedingChords, pinnedChord);

        // assert - the first candidate already matches, so the enumeration short-circuits after one evaluation
        hasPossibleChord.Should().BeTrue();
        _mockCompositionRule.Received(1).Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>());
    }

    [Test]
    public void HasPossibleChordForPartiallyVoicedChord_WhenNoRuleValidChordContainsThePinnedNotes_ReturnsFalse()
    {
        // arrange - the rule accepts every candidate, but none of them voices instrument One on the pinned E5
        var (precedingChords, _) = ArrangePartiallyVoicedCandidates();

        var unmatchablePinnedChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.E5, MusicalTimeSpan.Half)
        ]);

        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(true);

        // act
        var hasPossibleChord = _compositionStrategy.HasPossibleChordForPartiallyVoicedChord(precedingChords, unmatchablePinnedChord);

        // assert
        hasPossibleChord.Should().BeFalse();
    }

    [Test]
    public void GenerateInitialChord_RetriesUntilTheCompositionRuleAcceptsACandidate()
    {
        // arrange - the rule rejects the first two candidates and accepts the third; every candidate must be
        // evaluated against an empty preceding-chord context since no chords precede the initial chord
        var evaluatedChords = new List<BaroquenChord>();

        _mockCompositionRule.Evaluate(
                Arg.Is<IReadOnlyList<BaroquenChord>>(static precedingChords => precedingChords.Count == 0),
                Arg.Do<BaroquenChord>(evaluatedChords.Add))
            .Returns(false, false, true);

        // act
        var initialChord = _compositionStrategy.GenerateInitialChord();

        // assert
        evaluatedChords.Should().HaveCount(3);
        initialChord.Should().BeSameAs(evaluatedChords[^1]);
    }

    [Test]
    public void GenerateInitialChord_WhenNoCompliantCandidateCanBeFound_ReturnsTheLastCandidate()
    {
        // arrange - the rule rejects every candidate, so the bounded retry exhausts its attempts, logs a
        // warning, and degrades to the legacy behavior of using an unvalidated voicing
        const int maxInitialChordAttempts = 3;

        _mockLogger.IsEnabled(LogLevel.Warning).Returns(true);

        var compositionStrategy = new CompositionStrategy(
            _mockChordChoiceEnumerator,
            _mockCompositionRule,
            _mockLogger,
            _compositionConfiguration,
            new ThreadLocalRandomProvider(),
            maxInitialChordAttempts: maxInitialChordAttempts
        );

        var evaluatedChords = new List<BaroquenChord>();

        _mockCompositionRule.Evaluate(
                Arg.Any<IReadOnlyList<BaroquenChord>>(),
                Arg.Do<BaroquenChord>(evaluatedChords.Add))
            .Returns(false);

        // act
        var initialChord = compositionStrategy.GenerateInitialChord();

        // assert
        evaluatedChords.Should().HaveCount(maxInitialChordAttempts);
        initialChord.Should().BeSameAs(evaluatedChords[^1]);
    }

    [Test]
    public void GenerateInitialChord_WhenStartingNoteCannotBeFound_Throws()
    {
        // arrange - instrument range contains only F#3, which is not in C Major
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(
                    Instrument.One,
                    Notes.FSharp3,
                    Notes.FSharp3,
                    InstrumentConfiguration.DefaultMinVelocity,
                    InstrumentConfiguration.DefaultMaxVelocity,
                    GeneralMidi2Program.AcousticGrandPiano,
                    ConfigurationStatus.Enabled
                )
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100
        );

        var strategy = new CompositionStrategy(
            _mockChordChoiceEnumerator,
            _mockCompositionRule,
            _mockLogger,
            compositionConfiguration,
            new ThreadLocalRandomProvider()
        );

        // act
        var act = () => strategy.GenerateInitialChord();

        // assert
        act.Should().Throw<CouldNotFindStartingNoteForInstrumentException>();
    }

    // Audit: search-scoring-random-4 - three voices confined to C4..D4 can only sound C; once two voices have taken
    // it, the third voice's fallback keeps drawing the saturated C and the starting-note loop never exits. The
    // strategy must terminate (by throwing CouldNotFindStartingNoteForInstrumentException) instead of spinning.
    [Test]
    public void GenerateInitialChord_WhenEveryInRangeTriadNoteIsAlreadyDoubled_TerminatesByThrowing()
    {
        // arrange - built the same way as GenerateInitialChord_WhenStartingNoteCannotBeFound_Throws
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.One, Notes.C4, Notes.D4, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
                new(Instrument.Two, Notes.C4, Notes.D4, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
                new(Instrument.Three, Notes.C4, Notes.D4, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
            },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100
        );

        var strategy = new CompositionStrategy(
            _mockChordChoiceEnumerator,
            _mockCompositionRule,
            _mockLogger,
            compositionConfiguration,
            new ThreadLocalRandomProvider()
        );

        // act - run on a worker so a hang surfaces as a timed-out wait rather than a stuck test run
        var task = Task.Run(() => strategy.GenerateInitialChord());
        var completed = task.Wait(TimeSpan.FromSeconds(5));

        // assert
        completed.Should().BeTrue("the starting-note search must terminate when no in-range triad note can take another voice");

        var act = () => task.GetAwaiter().GetResult();

        act.Should().Throw<CouldNotFindStartingNoteForInstrumentException>();
    }

    /// <summary>
    ///     Arranges the enumerator to yield three candidates from a common preceding chord - an oblique candidate
    ///     that keeps instrument One on C4, and two candidates that move every voice by five and one scale steps
    ///     respectively - and returns the preceding chords along with a chord pinning instrument One to C4.
    /// </summary>
    private (List<BaroquenChord> PrecedingChords, BaroquenChord PinnedChord) ArrangePartiallyVoicedCandidates()
    {
        var precedingChords = new List<BaroquenChord>
        {
            new([
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
                new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
            ])
        };

        var chordChoices = new List<ChordChoice>
        {
            new([
                new NoteChoice(Instrument.One, NoteMotion.Oblique, 0),
                new NoteChoice(Instrument.Two, NoteMotion.Oblique, 0),
                new NoteChoice(Instrument.Three, NoteMotion.Oblique, 0),
                new NoteChoice(Instrument.Four, NoteMotion.Oblique, 0)
            ]),
            new([
                new NoteChoice(Instrument.One, NoteMotion.Ascending, 5),
                new NoteChoice(Instrument.Two, NoteMotion.Descending, 5),
                new NoteChoice(Instrument.Three, NoteMotion.Ascending, 5),
                new NoteChoice(Instrument.Four, NoteMotion.Descending, 5)
            ]),
            new([
                new NoteChoice(Instrument.One, NoteMotion.Ascending, 1),
                new NoteChoice(Instrument.Two, NoteMotion.Descending, 1),
                new NoteChoice(Instrument.Three, NoteMotion.Ascending, 1),
                new NoteChoice(Instrument.Four, NoteMotion.Descending, 1)
            ])
        };

        var candidates = chordChoices
            .Select(chordChoice => (ChordChoice: chordChoice, Chord: precedingChords[^1].ApplyChordChoice(_compositionConfiguration.Scale, chordChoice, _compositionConfiguration.DefaultNoteTimeSpan)))
            .ToList();

        _mockChordChoiceEnumerator.EnumerateCandidates(Arg.Any<BaroquenChord>()).Returns(candidates);

        var pinnedChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        ]);

        return (precedingChords, pinnedChord);
    }
}
