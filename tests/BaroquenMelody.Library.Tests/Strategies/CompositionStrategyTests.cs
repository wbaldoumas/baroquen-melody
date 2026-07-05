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
}
