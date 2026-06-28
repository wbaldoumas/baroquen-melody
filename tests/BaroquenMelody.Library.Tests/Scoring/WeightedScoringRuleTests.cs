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
internal sealed class WeightedScoringRuleTests
{
    [Test]
    [TestCase(2.5d, 3, 7.5d)]
    [TestCase(1d, 0, 0d)]
    [TestCase(0d, 5, 0d)]
    public void Score_MultipliesTheInnerPenaltyByTheWeight(double innerPenalty, int weight, double expectedPenalty)
    {
        // arrange
        var precedingChords = new List<BaroquenChord> { BuildChord(Notes.C4) };
        var nextChord = BuildChord(Notes.D4);

        var mockScoringRule = Substitute.For<IScoringRule>();

        mockScoringRule.Score(precedingChords, nextChord).Returns(innerPenalty);

        var weightedScoringRule = new WeightedScoringRule(mockScoringRule, weight);

        // act
        var penalty = weightedScoringRule.Score(precedingChords, nextChord);

        // assert
        penalty.Should().Be(expectedPenalty);
    }

    private static BaroquenChord BuildChord(Note note) => new([new BaroquenNote(Instrument.One, note, MusicalTimeSpan.Half)]);
}
