using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Compositions.Strategies;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using System.Numerics;

namespace BaroquenMelody.Library.Tests.Compositions.Strategies;

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

        _compositionConfiguration = Configurations.GetCompositionConfiguration();

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

            // since there are four voices but only three unique notes in a C Major chord, there should be at least one voice with a repeated note
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
                new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Tenor, Notes.G2, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Bass, Notes.C2, MusicalTimeSpan.Half)
            ])
        };

        var goodChordChoice = new ChordChoice([
            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0),
            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0),
            new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0),
            new NoteChoice(Voice.Bass, NoteMotion.Oblique, 0)
        ]);

        var otherGoodChordChoice = new ChordChoice([
            new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 1),
            new NoteChoice(Voice.Alto, NoteMotion.Descending, 1),
            new NoteChoice(Voice.Tenor, NoteMotion.Ascending, 1),
            new NoteChoice(Voice.Bass, NoteMotion.Descending, 1)
        ]);

        var badChordChoice = new ChordChoice([
            new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Alto, NoteMotion.Descending, 5),
            new NoteChoice(Voice.Tenor, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Bass, NoteMotion.Descending, 5)
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
        possibleChordChoices
            .Should()
            .HaveCount(2)
            .And.Contain(goodChordChoice)
            .And.Contain(otherGoodChordChoice)
            .And.NotContain(badChordChoice);
    }

    [Test]
    public void GetPossibleChordsForPartiallyVoicedChords_returns_expected_chords()
    {
        // arrange
        var precedingChords = new List<BaroquenChord>
        {
            new([
                new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Tenor, Notes.G2, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Bass, Notes.C2, MusicalTimeSpan.Half)
            ])
        };

        var nextChord = new BaroquenChord([
                new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Alto, Notes.E3, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Tenor, Notes.G2, MusicalTimeSpan.Half),
                new BaroquenNote(Voice.Bass, Notes.C2, MusicalTimeSpan.Half)
            ]
        );

        var goodChordChoice = new ChordChoice([
            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0),
            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0),
            new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0),
            new NoteChoice(Voice.Bass, NoteMotion.Oblique, 0)
        ]);

        var otherGoodChordChoice = new ChordChoice([
            new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 1),
            new NoteChoice(Voice.Alto, NoteMotion.Descending, 1),
            new NoteChoice(Voice.Tenor, NoteMotion.Ascending, 1),
            new NoteChoice(Voice.Bass, NoteMotion.Descending, 1)
        ]);

        var badChordChoice = new ChordChoice([
            new NoteChoice(Voice.Soprano, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Alto, NoteMotion.Descending, 5),
            new NoteChoice(Voice.Tenor, NoteMotion.Ascending, 5),
            new NoteChoice(Voice.Bass, NoteMotion.Descending, 5)
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
        possibleChords.Should().ContainSingle();
    }
}
