using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Scoring;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Scoring;

[TestFixture]
internal sealed class AggregateScoringRuleTests
{
    [Test]
    public void Score_ReturnsZero_WhenThereAreNoScoringRules()
    {
        // arrange
        var aggregateScoringRule = new AggregateScoringRule([]);

        // act
        var penalty = aggregateScoringRule.Score([], BuildChord(Notes.C4));

        // assert
        penalty.Should().Be(0d);
    }

    [Test]
    public void Score_SumsThePenaltiesOfItsScoringRules()
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };
        var nextChord = BuildChord(Notes.D4);

        var mockScoringRuleA = Substitute.For<IScoringRule>();
        var mockScoringRuleB = Substitute.For<IScoringRule>();

        mockScoringRuleA.Score(precedingChords, nextChord).Returns(1.5d);
        mockScoringRuleB.Score(precedingChords, nextChord).Returns(2d);

        var aggregateScoringRule = new AggregateScoringRule([mockScoringRuleA, mockScoringRuleB]);

        // act
        var penalty = aggregateScoringRule.Score(precedingChords, nextChord);

        // assert
        penalty.Should().Be(3.5d);
    }

    private static BaroquenChord BuildChord(Note note) => new([new BaroquenNote(Instrument.One, note, MusicalTimeSpan.Half)]);
}
