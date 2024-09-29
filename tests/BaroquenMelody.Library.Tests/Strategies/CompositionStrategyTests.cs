using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Strategies;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Numerics;

namespace BaroquenMelody.Library.Tests.Strategies;

[TestFixture]
internal sealed class CompositionStrategyTests
{
    private static readonly BigInteger MockChordChoiceCount = 5;

    private IChordChoiceRepository _mockChordChoiceRepository = default!;

    private ICompositionRule _mockCompositionRule = default!;

    private ILogger _mockLogger = default!;

    private CompositionStrategy _compositionStrategy = default!;

    private CompositionConfiguration _compositionConfiguration = default!;

    [SetUp]
    public void Setup()
    {
        _mockChordChoiceRepository = Substitute.For<IChordChoiceRepository>();
        _mockCompositionRule = Substitute.For<ICompositionRule>();
        _mockLogger = Substitute.For<ILogger>();

        _mockChordChoiceRepository.Count.Returns(MockChordChoiceCount);

        _compositionConfiguration = TestCompositionConfigurations.Get();

        _compositionStrategy = new CompositionStrategy(
            _mockChordChoiceRepository,
            _mockCompositionRule,
            _mockLogger,
            _compositionConfiguration
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

        _mockChordChoiceRepository.Count.Returns(mockChordChoices.Count);

        var mockChordChoiceRepositoryReturn = new List<ChordChoice>();

        // Some fun times mocking here and below to account for recursion and look-ahead depth
        foreach (var outerChordChoice in mockChordChoices)
        {
            mockChordChoiceRepositoryReturn.Add(outerChordChoice);

            foreach (var innerChordChoice in mockChordChoices)
            {
                mockChordChoiceRepositoryReturn.Add(innerChordChoice);
                mockChordChoiceRepositoryReturn.AddRange(mockChordChoices);
            }
        }

        _mockChordChoiceRepository
            .GetChordChoice(Arg.Any<BigInteger>())
            .Returns(
                mockChordChoiceRepositoryReturn[0],
                mockChordChoiceRepositoryReturn.ToArray()[1..]
            );

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

        _mockChordChoiceRepository.Count.Returns(mockChordChoices.Count);

        var mockChordChoiceRepositoryReturn = new List<ChordChoice>();

        // Some fun times mocking here and below to account for recursion and look-ahead depth
        foreach (var outerChordChoice in mockChordChoices)
        {
            mockChordChoiceRepositoryReturn.Add(outerChordChoice);

            foreach (var innerChordChoice in mockChordChoices)
            {
                mockChordChoiceRepositoryReturn.Add(innerChordChoice);
                mockChordChoiceRepositoryReturn.AddRange(mockChordChoices);
            }
        }

        _mockChordChoiceRepository
            .GetChordChoice(Arg.Any<BigInteger>())
            .Returns(
                mockChordChoiceRepositoryReturn[0],
                mockChordChoiceRepositoryReturn.ToArray()[1..]
            );

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
}
