using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Scoring.Melodic;
using BaroquenMelody.Library.Viewpoints;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Scoring.Melodic;

[TestFixture]
internal sealed class MelodicScoringRuleAdapterTests
{
    private IMelodicScoringRule _mockMelodicScoringRule = null!;

    private MelodicScoringRuleAdapter _melodicScoringRuleAdapter = null!;

    [SetUp]
    public void SetUp()
    {
        _mockMelodicScoringRule = Substitute.For<IMelodicScoringRule>();
        _melodicScoringRuleAdapter = new MelodicScoringRuleAdapter(_mockMelodicScoringRule);
    }

    [Test]
    public void Score_SumsTheInnerRulePenaltyOverEveryVoice()
    {
        // arrange
        _mockMelodicScoringRule.Score(Arg.Any<MelodicLine>()).Returns(2d, 3d);

        var nextChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);

        // act
        var penalty = _melodicScoringRuleAdapter.Score([], nextChord);

        // assert
        penalty.Should().Be(5d);
    }

    [Test]
    public void Score_ProjectsOneMelodicLinePerVoice()
    {
        // arrange
        var precedingNote = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);
        var precedingChords = new List<BaroquenChord> { new([precedingNote]) };

        var nextChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);

        // act
        _melodicScoringRuleAdapter.Score(precedingChords, nextChord);

        // assert: one line per voice, in chord-note order, each projecting the same preceding chords
        var melodicLines = _mockMelodicScoringRule.ReceivedCalls()
            .Select(call => (MelodicLine)call.GetArguments()[0]!)
            .ToList();

        melodicLines.Select(melodicLine => melodicLine.NextNote).Should().Equal(nextChord.Notes);
        melodicLines[0].PrecedingNote(1).Should().BeSameAs(precedingNote);
        melodicLines[1].PrecedingNote(1).Should().BeNull();
    }
}
