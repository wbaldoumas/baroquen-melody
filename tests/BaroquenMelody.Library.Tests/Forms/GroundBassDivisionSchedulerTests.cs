using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Forms;

[TestFixture]
internal sealed class GroundBassDivisionSchedulerTests
{
    private static readonly IReadOnlyList<Note> GroundNotes = [Notes.C3, Notes.B2, Notes.A2, Notes.G2];

    [Test]
    public void ShouldHoldGroundLine_WithDefaultConfigurations_IsTrue()
    {
        // arrange - both governing configurations default on, even when absent from the configuration
        var scheduler = CreateScheduler(TestCompositionConfigurations.Get(3));

        // act & assert
        scheduler.ShouldHoldGroundLine().Should().BeTrue();
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(false, false)]
    public void AllRoles_WhenAGoverningSwitchIsOff_AnswerFalse(bool voiceRhythmEnabled, bool divisions)
    {
        // arrange - voice rhythm is the role machinery's master switch; Divisions narrows it for the form
        var configuration = TestCompositionConfigurations.Get(3) with
        {
            VoiceRhythmConfiguration = new VoiceRhythmConfiguration(voiceRhythmEnabled),
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, Divisions: divisions)
        };

        var scheduler = CreateScheduler(configuration);
        var plan = CreatePlan(configuration, statementCount: 4);

        // act & assert
        scheduler.ShouldHoldGroundLine().Should().BeFalse();
        scheduler.TryGetIntensity(plan, 1, out _).Should().BeFalse();
        scheduler.TryGetFloridInstrument(plan, 1, out _).Should().BeFalse();
        scheduler.TryGetTerraceOffset(plan, 1, out _).Should().BeFalse();
    }

    [Test]
    public void EscalationRoles_ForTheAnnouncement_AnswerFalse()
    {
        // arrange - statement zero is the solo announcement: it never escalates, but the ground line holds
        var configuration = TestCompositionConfigurations.Get(3);
        var scheduler = CreateScheduler(configuration);
        var plan = CreatePlan(configuration, statementCount: 4);

        // act & assert
        scheduler.TryGetIntensity(plan, 0, out _).Should().BeFalse();
        scheduler.TryGetFloridInstrument(plan, 0, out _).Should().BeFalse();
        scheduler.TryGetTerraceOffset(plan, 0, out _).Should().BeFalse();
        scheduler.ShouldHoldGroundLine().Should().BeTrue();
    }

    [Test]
    public void EscalationRoles_WithNoUpperVoices_AnswerFalse()
    {
        // arrange - a bass-only ground has no division voice: no escalation, no terrace, only the tread
        var configuration = TestCompositionConfigurations.Get(1);
        var scheduler = CreateScheduler(configuration);
        var plan = CreatePlan(configuration, statementCount: 4);

        // act & assert
        scheduler.TryGetIntensity(plan, 1, out _).Should().BeFalse();
        scheduler.TryGetFloridInstrument(plan, 1, out _).Should().BeFalse();
        scheduler.TryGetTerraceOffset(plan, 1, out _).Should().BeFalse();
        scheduler.ShouldHoldGroundLine().Should().BeTrue();
    }

    [TestCase(2, new[] { 140 })]
    [TestCase(3, new[] { 30, 140 })]
    [TestCase(4, new[] { 30, 85, 140 })]
    [TestCase(5, new[] { 30, 67, 104, 140 })]
    public void TryGetIntensity_RampsFromCalmToPeak_AnchoredAtTheFinalStatement(int statementCount, int[] expectedIntensities)
    {
        // arrange - the END anchor is the load-bearing choice: a two-statement plan's only accompanied
        // statement must take the peak, not the calm floor. Values pin the actual integer math.
        var configuration = TestCompositionConfigurations.Get(3);
        var scheduler = CreateScheduler(configuration);
        var plan = CreatePlan(configuration, statementCount);

        // act
        var intensities = Enumerable.Range(1, statementCount - 1)
            .Select(statementIndex =>
            {
                scheduler.TryGetIntensity(plan, statementIndex, out var intensity).Should().BeTrue();

                return intensity;
            })
            .ToArray();

        // assert
        intensities.Should().Equal(expectedIntensities);
    }

    [TestCase(2, new[] { 0 })]
    [TestCase(4, new[] { -5, -2, 0 })]
    [TestCase(5, new[] { -5, -3, -1, 0 })]
    public void TryGetTerraceOffset_StepsUpwardToZeroAtTheFinalStatement(int statementCount, int[] expectedOffsets)
    {
        // arrange - the terrace steps DOWNWARD from today's level (no upward velocity headroom exists at
        // default ranges), reaching zero exactly at the final statement so the arc grows into the close
        var configuration = TestCompositionConfigurations.Get(3);
        var scheduler = CreateScheduler(configuration);
        var plan = CreatePlan(configuration, statementCount);

        // act
        var offsets = Enumerable.Range(1, statementCount - 1)
            .Select(statementIndex =>
            {
                scheduler.TryGetTerraceOffset(plan, statementIndex, out var offset).Should().BeTrue();

                return offset;
            })
            .ToArray();

        // assert
        offsets.Should().Equal(expectedOffsets);
    }

    [Test]
    public void TryGetFloridInstrument_RotatesOverTheUpperVoices_NeverTheBass()
    {
        // arrange - three voices, bass Three: the division voice alternates One, Two, One, ...
        var configuration = TestCompositionConfigurations.Get(3);
        var scheduler = CreateScheduler(configuration);
        var plan = CreatePlan(configuration, statementCount: 6);

        // act
        var floridInstruments = Enumerable.Range(1, 5)
            .Select(statementIndex =>
            {
                scheduler.TryGetFloridInstrument(plan, statementIndex, out var floridInstrument).Should().BeTrue();

                return floridInstrument;
            })
            .ToArray();

        // assert
        floridInstruments.Should().Equal(Instrument.One, Instrument.Two, Instrument.One, Instrument.Two, Instrument.One);
    }

    [Test]
    public void TryGetFloridInstrument_WithASingleUpperVoice_IsTheSoloDivisionVoiceEveryStatement()
    {
        // arrange - two voices, bass Two: the one upper voice carries every statement's division
        var configuration = TestCompositionConfigurations.Get(2);
        var scheduler = CreateScheduler(configuration);
        var plan = CreatePlan(configuration, statementCount: 4);

        // act
        var floridInstruments = Enumerable.Range(1, 3)
            .Select(statementIndex =>
            {
                scheduler.TryGetFloridInstrument(plan, statementIndex, out var floridInstrument).Should().BeTrue();

                return floridInstrument;
            })
            .ToArray();

        // assert
        floridInstruments.Should().Equal(Instrument.One, Instrument.One, Instrument.One);
    }

    private static GroundBassDivisionScheduler CreateScheduler(CompositionConfiguration configuration) => new(configuration);

    private static GroundBassPlan CreatePlan(CompositionConfiguration configuration, int statementCount) => new(
        GroundBassPattern.Bank[0],
        configuration.Instruments[^1],
        GroundNotes,
        statementCount,
        MeasuresPerStatement: 2,
        [new TonalSection(NoteName.C, Mode.Ionian, FirstStatement: 0, LastStatement: statementCount - 1, GroundNotes)]);
}
