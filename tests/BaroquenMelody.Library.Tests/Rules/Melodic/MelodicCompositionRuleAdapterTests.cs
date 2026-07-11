using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rules.Melodic;
using BaroquenMelody.Library.Viewpoints;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rules.Melodic;

[TestFixture]
internal sealed class MelodicCompositionRuleAdapterTests
{
    private IMelodicCompositionRule _mockMelodicCompositionRule = null!;

    private MelodicCompositionRuleAdapter _melodicCompositionRuleAdapter = null!;

    [SetUp]
    public void SetUp()
    {
        _mockMelodicCompositionRule = Substitute.For<IMelodicCompositionRule>();
        _melodicCompositionRuleAdapter = new MelodicCompositionRuleAdapter(_mockMelodicCompositionRule);
    }

    [Test]
    public void Evaluate_PassesWhenEveryVoiceLinePasses()
    {
        // arrange
        _mockMelodicCompositionRule.Evaluate(Arg.Any<MelodicLine>()).Returns(true);

        var nextChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);

        // act
        var isValid = _melodicCompositionRuleAdapter.Evaluate([], nextChord);

        // assert
        isValid.Should().BeTrue();
        _mockMelodicCompositionRule.ReceivedCalls().Should().HaveCount(2);
    }

    [Test]
    public void Evaluate_FailsAndShortCircuitsOnTheFirstFailingVoiceLine()
    {
        // arrange
        _mockMelodicCompositionRule.Evaluate(Arg.Any<MelodicLine>()).Returns(false, true);

        var nextChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);

        // act
        var isValid = _melodicCompositionRuleAdapter.Evaluate([], nextChord);

        // assert
        isValid.Should().BeFalse();
        _mockMelodicCompositionRule.ReceivedCalls().Should().ContainSingle();
    }

    [Test]
    public void Evaluate_ProjectsOneMelodicLinePerVoice()
    {
        // arrange
        _mockMelodicCompositionRule.Evaluate(Arg.Any<MelodicLine>()).Returns(true);

        var precedingNote = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);
        var precedingChords = new List<BaroquenChord> { new([precedingNote]) };

        var nextChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half)
        ]);

        // act
        _melodicCompositionRuleAdapter.Evaluate(precedingChords, nextChord);

        // assert: one line per voice, in chord-note order, each projecting the same preceding chords
        var melodicLines = _mockMelodicCompositionRule.ReceivedCalls()
            .Select(call => (MelodicLine)call.GetArguments()[0]!)
            .ToList();

        melodicLines.Select(melodicLine => melodicLine.NextNote).Should().Equal(nextChord.Notes);
        melodicLines[0].PrecedingNote(1).Should().BeSameAs(precedingNote);
        melodicLines[1].PrecedingNote(1).Should().BeNull();
    }
}
