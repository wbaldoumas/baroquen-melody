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
        firstPlan.Sections.Should().HaveCount(secondPlan.Sections.Count);
    }

    [Test]
    public void CreatePlan_InTheLiftedModeWithEnoughStatements_PlansARelativeKeyJourney()
    {
        // arrange: the test-shaped C2-C3 bass hosts the tetrachord in C Ionian (C3 down to G2) and in the
        // relative A Aeolian (A2 down to E2), and both seam motions are singable (G2 up a step to A2;
        // E2 up a consonant minor sixth to C3), so thirteen statements plan as home, relative middle,
        // home return - about a third foreign, placed centrally, without any additional draw.
        var configuration = TestCompositionConfigurations.Get(3, 25) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.DescendingTetrachord)
        };
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.Sections.Should().HaveCount(3);
        plan.Sections[0].Should().Match<TonalSection>(static section =>
            section.Tonic == NoteName.C && section.Mode == Mode.Ionian && section.FirstStatement == 0 && section.LastStatement == 3);
        plan.Sections[1].Should().Match<TonalSection>(static section =>
            section.Tonic == NoteName.A && section.Mode == Mode.Aeolian && section.FirstStatement == 4 && section.LastStatement == 7);
        plan.Sections[2].Should().Match<TonalSection>(static section =>
            section.Tonic == NoteName.C && section.Mode == Mode.Ionian && section.FirstStatement == 8 && section.LastStatement == 12);
        plan.Sections[0].BassNotes.Should().Equal(plan.BassNotes, "the home sections carry the home rendering");
        plan.Sections[2].BassNotes.Should().Equal(plan.BassNotes, "the home sections carry the home rendering");
        plan.Sections[1].BassNotes.Should().Equal(Notes.A2, Notes.G2, Notes.F2, Notes.E2);
        randomProvider.Received(1).Next(1);
    }

    [Test]
    public void CreatePlan_InAeolian_JourneysToTheRelativeMajor()
    {
        // arrange: the mirrored direction - an A Aeolian home renders the tetrachord A2 down to E2 and its
        // relative C Ionian renders it C3 down to G2, with the seams inverted (E2 up a minor sixth out,
        // G2 up a step back home).
        var configuration = TestCompositionConfigurations.Get(3, 25, NoteName.A, Mode.Aeolian) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.DescendingTetrachord)
        };
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.Sections.Should().HaveCount(3);
        plan.Sections[1].Should().Match<TonalSection>(static section => section.Tonic == NoteName.C && section.Mode == Mode.Ionian);
        plan.Sections[1].BassNotes.Should().Equal(Notes.C3, Notes.B2, Notes.A2, Notes.G2);
    }

    [Test]
    public void CreatePlan_WithModulationDisabled_PlansASingleHomeSection()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 25) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.DescendingTetrachord, Modulate: false)
        };
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.Sections.Should().ContainSingle();
        plan.Sections[0].Should().Match<TonalSection>(static section =>
            section.Tonic == NoteName.C && section.Mode == Mode.Ionian && section.FirstStatement == 0 && section.LastStatement == 12);
        randomProvider.Received(1).Next(1);
    }

    [Test]
    public void CreatePlan_InAModeWithoutARelativeLicense_StaysHome()
    {
        // arrange: Dorian is outside the lifted modes (the tonicization gate precedent), so even a
        // journey-sized composition plans a single home section.
        var configuration = TestCompositionConfigurations.Get(3, 25, NoteName.C, Mode.Dorian) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.DescendingTetrachord)
        };
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.Sections.Should().ContainSingle();
    }

    [Test]
    public void CreatePlan_WithFewerThanFourStatements_StaysHome()
    {
        // arrange: a ten-measure cadential ground plans three statements - not enough for the solo
        // announcement, an accompanied home statement, a foreign block, and a home return.
        var configuration = TestCompositionConfigurations.Get(3, 10) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.CadentialGround)
        };
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.StatementCount.Should().Be(3);
        plan.Sections.Should().ContainSingle();
    }

    [Test]
    public void CreatePlan_WhenTheRelativeKeyCannotHostThePattern_StaysHomeWithoutExtraDraws()
    {
        // arrange: the romanesca anchors on C3 at home (its octave span bottoming on C2), but the relative
        // A Aeolian offers only the A2 anchor, whose octave would run to A1 - below the bass floor. The
        // journey must be declined without disturbing the one-draw contract.
        var configuration = TestCompositionConfigurations.Get(3, 25) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.Romanesca)
        };
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.Sections.Should().ContainSingle();
        randomProvider.Received(1).Next(1);
    }

    [Test]
    public void CreatePlan_WhenTheSeamMotionWouldBeADissonantLeap_StaysHome()
    {
        // arrange: over the G3-B4 tenor bass an A Aeolian tetrachord anchors on A4 while the relative
        // C Ionian anchors on C4, so the return seam would leap a major ninth from G3 up to A4 - a pinned
        // bass motion the melodic net rejects, which the design probe showed starves the seam onset. The
        // planner must decline the journey rather than plan an uncomposable seam.
        var configuration = BuildConfiguration(BuildTenorBassInstruments(), NoteName.A, Mode.Aeolian, GroundBass.DescendingTetrachord);
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.BassNotes[0].Should().Be(Notes.A4);
        plan.Sections.Should().ContainSingle();
    }

    [TestCase(8, 4, 2, 2, TestName = "CreatePlan_FourStatements_PlacesOneForeignStatementThird")]
    [TestCase(10, 5, 2, 2, TestName = "CreatePlan_FiveStatements_PlacesOneForeignStatementThird")]
    [TestCase(12, 6, 2, 3, TestName = "CreatePlan_SixStatements_PlacesTwoForeignStatementsCentrally")]
    [TestCase(25, 13, 4, 7, TestName = "CreatePlan_ThirteenStatements_PlacesFourForeignStatementsCentrally")]
    public void CreatePlan_PlacesTheForeignBlockByFormula(int minimumMeasures, int expectedStatementCount, int expectedFirstForeign, int expectedLastForeign)
    {
        // arrange: the block is a pure function of the statement count - about a third of the statements,
        // centered, lead of at least two, tail of at least one - so the plan costs no draw beyond the
        // pattern draw whatever the composition's length.
        var configuration = TestCompositionConfigurations.Get(3, minimumMeasures) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.DescendingTetrachord)
        };
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var planner = new GroundBassPlanner(configuration, randomProvider);

        // act
        var plan = planner.CreatePlan();

        // assert
        plan.Should().NotBeNull();
        plan!.StatementCount.Should().Be(expectedStatementCount);
        plan.Sections.Should().HaveCount(3);
        plan.Sections[1].FirstStatement.Should().Be(expectedFirstForeign);
        plan.Sections[1].LastStatement.Should().Be(expectedLastForeign);
        plan.Sections[0].LastStatement.Should().Be(expectedFirstForeign - 1);
        plan.Sections[2].FirstStatement.Should().Be(expectedLastForeign + 1);
        plan.Sections[2].LastStatement.Should().Be(expectedStatementCount - 1);
    }

    [Test]
    public void SectionForStatement_PartitionsEveryStatementIntoExactlyOneSection()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 25) with
        {
            GroundBassConfiguration = new GroundBassConfiguration(Enabled: true, GroundBass.DescendingTetrachord)
        };
        var randomProvider = Substitute.For<IRandomProvider>();

        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var plan = new GroundBassPlanner(configuration, randomProvider).CreatePlan();

        // assert
        plan.Should().NotBeNull();

        for (var statementIndex = 0; statementIndex < plan!.StatementCount; ++statementIndex)
        {
            var owningSections = plan.Sections
                .Where(section => statementIndex >= section.FirstStatement && statementIndex <= section.LastStatement)
                .ToList();

            owningSections.Should().ContainSingle($"statement {statementIndex} must belong to exactly one section");
            plan.SectionForStatement(statementIndex).Should().BeSameAs(owningSections[0]);
        }
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
