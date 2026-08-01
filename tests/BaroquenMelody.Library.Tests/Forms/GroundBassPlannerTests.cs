using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NSubstitute;
using NUnit.Framework;
using Mode = BaroquenMelody.Library.MusicTheory.Enums.Mode;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Forms;

/// <summary>
///     Pins the planner's feasibility filter, anchor centering, statement arithmetic, and draw discipline.
///     Range fixtures mirror the design probe's findings: a G3-B4 bass hosts only the tetrachord in C Ionian
///     but every pattern in A Aeolian, the test-shaped C2-C3 bass hosts all three, and the default ranges are
///     pinned to host the whole bank so future range changes stay honest about the ground repertoire.
/// </summary>
[TestFixture]
internal sealed class GroundBassPlannerTests
{
    [Test]
    public void CreatePlan_WithTheTestShapedBassRange_RendersATonicAnchoredPattern()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 25);
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.BassInstrument.Should().Be(Instrument.Three, "the ground goes to the lowest configured voice");
        plan.Pattern.Identifier.Should().Be(GroundBass.DescendingTetrachord);
        plan.BassNotes.Should().HaveCount(plan.Pattern.ScaleStepOffsets.Count);
        plan.BassNotes[0].NoteName.Should().Be(NoteName.C, "the anchor is the tonic");
        plan.BassNotes.Should().OnlyContain(note => note.NoteNumber >= Notes.C2.NoteNumber && note.NoteNumber <= Notes.C3.NoteNumber, "every rendered note must sit inside the bass range");
        randomProvider.Received(1).Next(3);
    }

    [Test]
    public void CreatePlan_WithABassRangeTooNarrowBelowItsTonic_OnlyTheTetrachordIsFeasible()
    {
        // arrange: a G3-B4 bass in C Ionian holds a single tonic (C4) with just three scale steps below
        // it in range, so the octave-descending grounds cannot anchor and only the tetrachord survives
        // the feasibility filter.
        var configuration = BuildConfiguration(BuildTenorBassInstruments(), NoteName.C, Mode.Ionian);
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.Pattern.Identifier.Should().Be(GroundBass.DescendingTetrachord);
        plan.BassNotes.Should().Equal(Notes.C4, Notes.B3, Notes.A3, Notes.G3);
        randomProvider.Received(1).Next(1);
    }

    [Test]
    public void CreatePlan_WithATonicAtopTheBassRange_RendersTheRomanescaFromTheHighTonic()
    {
        // arrange: in A Aeolian the same G3-B4 bass hosts the tonic A4 with a full octave below it,
        // so all three bank patterns are feasible; the mocked draw picks the romanesca.
        var configuration = BuildConfiguration(BuildTenorBassInstruments(), NoteName.A, Mode.Aeolian);
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(1);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.Pattern.Identifier.Should().Be(GroundBass.Romanesca);
        plan.BassNotes.Should().Equal(Notes.A4, Notes.E4, Notes.F4, Notes.C4, Notes.D4, Notes.A3, Notes.D4, Notes.E4);
        randomProvider.Received(1).Next(3);
    }

    [TestCase(NoteName.C, Mode.Ionian, TestName = "CreatePlan_WithTheDefaultEnabledVoices_HostsEveryBankPatternInIonian")]
    [TestCase(NoteName.A, Mode.Aeolian, TestName = "CreatePlan_WithTheDefaultEnabledVoices_HostsEveryBankPatternInAeolian")]
    public void CreatePlan_WithTheDefaultEnabledVoices_HostsEveryBankPattern(NoteName tonic, Mode mode)
    {
        // arrange: the default ranges anchor each lower voice's floor a full octave below an in-range
        // tonic-capable pitch, so the default bass (Three, C3-B4) hosts the whole bank in both lifted
        // modes. The draw's upper bound equaling the bank size IS the feasibility pin - it keeps future
        // default-range changes honest about the ground repertoire.
        var enabledDefaults = InstrumentConfiguration.DefaultConfigurations.Values.Where(static c => c.IsEnabled).ToHashSet();
        var configuration = BuildConfiguration(enabledDefaults, tonic, mode);
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.BassInstrument.Should().Be(Instrument.Three);
        randomProvider.Received(1).Next(GroundBassPattern.Bank.Count);
    }

    [Test]
    public void CreatePlan_WithEveryDefaultVoiceEnabled_HostsEveryBankPatternInTheBass()
    {
        // arrange: with the fourth voice enabled it becomes the ground carrier; its C2-A3 range holds the
        // C2-C3 tonic octave, so the whole bank anchors on C3 in the default key.
        var allDefaults = InstrumentConfiguration.DefaultConfigurations.Values
            .Select(static c => c with { Status = ConfigurationStatus.Enabled })
            .ToHashSet();
        var configuration = BuildConfiguration(allDefaults, NoteName.C, Mode.Ionian);
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.BassInstrument.Should().Be(Instrument.Four);
        plan.BassNotes[0].Should().Be(Notes.C3);
        randomProvider.Received(1).Next(GroundBassPattern.Bank.Count);
    }

    [Test]
    public void CreatePlan_WithAConfiguredPattern_PlansExactlyThatPattern()
    {
        // arrange: the test-shaped C2-C3 bass hosts the whole bank, but the configuration pins the
        // cadential ground - the draw's upper bound collapses to the pinned singleton, keeping the
        // one-draw-per-plan contract so the random stream stays aligned across selections.
        var configuration = BuildConfiguration(BuildTenorBassInstruments(), NoteName.A, Mode.Aeolian, GroundBass.CadentialGround);
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.Pattern.Identifier.Should().Be(GroundBass.CadentialGround);
        randomProvider.Received(1).Next(1);
    }

    [Test]
    public void CreatePlan_WithAConfiguredPatternTheBassCannotHost_ReturnsNullWithoutDrawing()
    {
        // arrange: a G3-B4 bass in C Ionian hosts only the tetrachord, so pinning the romanesca must
        // yield no plan (the fugue fallback) rather than silently substituting a different ground.
        var configuration = BuildConfiguration(BuildTenorBassInstruments(), NoteName.C, Mode.Ionian, GroundBass.Romanesca);
        var randomProvider = Substitute.For<IRandomProvider>();
        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().BeNull();
        randomProvider.DidNotReceive().Next(Arg.Any<int>());
    }

    [Test]
    public void CreatePlan_ChoosesTheAnchorNearestTheRangeCenterTieBreakingLow()
    {
        // arrange: a G3-F5 bass puts C4 and C5 exactly equidistant from the range center, so the lower
        // anchor must win the tie.
        var configuration = BuildConfiguration(BuildTwoVoiceInstruments(Notes.G3, Notes.F5), NoteName.C, Mode.Ionian);
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.Pattern.Identifier.Should().Be(GroundBass.DescendingTetrachord);
        plan.BassNotes[0].Should().Be(Notes.C4);
    }

    [Test]
    public void CreatePlan_WhenNoPatternCanAnchorInTheBassRange_ReturnsNullWithoutDrawing()
    {
        // arrange: a B2-B3 bass contains a single tonic (C3) with only one scale step below it in range,
        // so no bank pattern can anchor.
        var configuration = BuildConfiguration(BuildTwoVoiceInstruments(Notes.B2, Notes.B3), NoteName.C, Mode.Ionian);
        var randomProvider = Substitute.For<IRandomProvider>();
        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().BeNull();
        randomProvider.DidNotReceive().Next(Arg.Any<int>());
    }

    [TestCase(0, 25, 2, 13, TestName = "CreatePlan_TetrachordAtTwentyFiveMeasures_StatesThirteenTimes")]
    [TestCase(0, 24, 2, 12, TestName = "CreatePlan_TetrachordAtAnExactFit_StatesExactlyEnoughTimes")]
    [TestCase(0, 1, 2, 2, TestName = "CreatePlan_ATinyComposition_StillStatesTheGroundTwice")]
    [TestCase(1, 25, 4, 7, TestName = "CreatePlan_RomanescaAtTwentyFiveMeasures_StatesSevenTimes")]
    public void CreatePlan_ComputesStatementCountFromTheMinimumMeasures(int draw, int minimumMeasures, int expectedMeasuresPerStatement, int expectedStatementCount)
    {
        // arrange: the test-shaped C2-C3 bass hosts every bank pattern, so the mocked draw selects freely.
        var configuration = TestCompositionConfigurations.Get(3, minimumMeasures);
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(draw);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.MeasuresPerStatement.Should().Be(expectedMeasuresPerStatement);
        plan.StatementCount.Should().Be(expectedStatementCount);
        (plan.StatementCount * plan.MeasuresPerStatement).Should().BeGreaterThanOrEqualTo(Math.Min(minimumMeasures, 2 * plan.MeasuresPerStatement), "the statements must cover the configured minimum or the two-statement floor");
    }

    [Test]
    public void CreatePlan_AtTheBottomOfTheScaleGamut_SkipsAnchorsWhoseOffsetsRunOffTheNoteList()
    {
        // arrange: a bass range hugging the lowest playable octave contains the scale list's very first
        // tonic, whose downward offsets would index before the list starts - the planner must skip it and
        // anchor on the octave above.
        var lowestTonic = Note.Get(NoteName.C, -1);
        var configuration = BuildConfiguration(BuildTwoVoiceInstruments(lowestTonic, Note.Get(NoteName.C, 0)), NoteName.C, Mode.Ionian);
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.BassNotes[0].Should().Be(Note.Get(NoteName.C, 0), "the gamut-bottom tonic cannot host a descending ground");
    }

    [Test]
    public void CreatePlan_WithTheSameSeed_IsDeterministic()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 25);
        var firstPlan = new GroundBassPlanner(configuration, new SeededRandomProvider(1234)).CreatePlan();
        var secondPlan = new GroundBassPlanner(configuration, new SeededRandomProvider(1234)).CreatePlan();

        // assert
        firstPlan.Should().NotBeNull();
        secondPlan.Should().NotBeNull();
        firstPlan!.Pattern.Identifier.Should().Be(secondPlan!.Pattern.Identifier);
        firstPlan.BassNotes.Should().Equal(secondPlan.BassNotes);
        firstPlan.StatementCount.Should().Be(secondPlan.StatementCount);
    }

    private static HashSet<InstrumentConfiguration> BuildTenorBassInstruments() =>
    [
        new InstrumentConfiguration(Instrument.One, Notes.C5, Notes.E6, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
        new InstrumentConfiguration(Instrument.Two, Notes.E4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
        new InstrumentConfiguration(Instrument.Three, Notes.G3, Notes.B4, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
    ];

    private static HashSet<InstrumentConfiguration> BuildTwoVoiceInstruments(Note bassMinNote, Note bassMaxNote) =>
    [
        new InstrumentConfiguration(Instrument.One, Notes.C5, Notes.E6, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
        new InstrumentConfiguration(Instrument.Two, bassMinNote, bassMaxNote, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
    ];

    private static CompositionConfiguration BuildConfiguration(HashSet<InstrumentConfiguration> instrumentConfigurations, NoteName tonic, Mode mode, GroundBass? pattern = null) => new(
        instrumentConfigurations,
        PhrasingConfiguration.Default,
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        tonic,
        mode,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        MinimumMeasures: 25,
        GroundBassConfiguration: new GroundBassConfiguration(Enabled: true, pattern)
    );
}
