using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Enums.Extensions;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Ornamentation;

[TestFixture]
internal sealed class SuspensionApplicatorTests
{
    private IWeightedRandomBooleanGenerator _mockWeightedRandomBooleanGenerator = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    [SetUp]
    public void SetUp()
    {
        _mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();
        _mockWeightedRandomBooleanGenerator.IsTrue(Arg.Any<int>()).Returns(true);

        _compositionConfiguration = TestCompositionConfigurations.Get(2);
    }

    [Test]
    public void ApplySuspensions_AtAMidMeasureHarmonicChange_TiesThePreparationAndDelaysTheResolution()
    {
        // arrange - the classic 4-3: F4 over an F harmony is held into a C-E chord (F clashes) and resolves to E4
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.D3), Chord(Notes.E4, Notes.C3), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        var preparation = composition.Measures[0].Beats[1].Chord[Instrument.One];
        var resolution = composition.Measures[0].Beats[2].Chord[Instrument.One];

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert
        preparation.OrnamentationType.Should().Be(OrnamentationType.Suspension);
        preparation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Half + MusicalTimeSpan.Quarter);
        resolution.OrnamentationType.Should().Be(OrnamentationType.SuspensionResolution);
        resolution.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
    }

    [Test]
    public void ApplySuspensions_AcrossTheBarline_TiesThePreparationIntoTheNextDownbeat()
    {
        // arrange - the preparation is the final beat of measure 0; the resolution is the downbeat of measure 1
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.D3)]),
            BuildMeasure([Chord(Notes.E4, Notes.C3), Chord(Notes.G4, Notes.E3), Chord(Notes.G4, Notes.E3), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        var preparation = composition.Measures[0].Beats[^1].Chord[Instrument.One];
        var resolution = composition.Measures[1].Beats[0].Chord[Instrument.One];

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert
        preparation.OrnamentationType.Should().Be(OrnamentationType.Suspension);
        preparation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Half + MusicalTimeSpan.Quarter);
        resolution.OrnamentationType.Should().Be(OrnamentationType.SuspensionResolution);
        resolution.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
    }

    [Test]
    public void ApplySuspensions_WhenBothVoicesAreEligible_SuspendsBothIndependently()
    {
        // arrange - a double suspension: One holds F4 over C-E and Two holds D3 over C-E in parallel
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F2), Chord(Notes.F4, Notes.D3), Chord(Notes.E4, Notes.C3), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert
        composition.Measures[0].Beats[1].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.Suspension);
        composition.Measures[0].Beats[1].Chord[Instrument.Two].OrnamentationType.Should().Be(OrnamentationType.Suspension);
        composition.Measures[0].Beats[2].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.SuspensionResolution);
        composition.Measures[0].Beats[2].Chord[Instrument.Two].OrnamentationType.Should().Be(OrnamentationType.SuspensionResolution);
    }

    [Test]
    public void ApplySuspensions_NeverTouchesTheFinalMeasure()
    {
        // arrange - both eligible sites resolve within or into the final measure, so neither may fire
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.D3)]),
            BuildMeasure([Chord(Notes.E4, Notes.C3), Chord(Notes.F4, Notes.D3), Chord(Notes.E4, Notes.C3), Chord(Notes.G4, Notes.E3)]));

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert
        AllNotes(composition).Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.None);
    }

    [Test]
    public void ApplySuspensions_WhenThePreparationPitchSurvivesIntoTheNewChord_DoesNotSuspend()
    {
        // arrange - F4 over a D-F harmony: the F is a common tone, not a dissonance
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.D3), Chord(Notes.E4, Notes.F3), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert
        AllNotes(composition).Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.None);
    }

    [TestCase(0)]
    [TestCase(2)]
    public void ApplySuspensions_WhenTheVoiceDoesNotDescendByExactlyOneStep_DoesNotSuspend(int scaleStepsDown)
    {
        // arrange - a repeated note (0) or a downward leap of a third (2) is not a suspension figure
        var scaleNotes = _compositionConfiguration.Scale.GetNotes();
        var preparationNote = Notes.F4;
        var resolutionNote = scaleNotes[scaleNotes.IndexOf(preparationNote) - scaleStepsDown];

        // the bass holds D3 throughout so only the soprano's motion is under test
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.D3), Chord(preparationNote, Notes.D3), Chord(resolutionNote, Notes.D3), Chord(Notes.G4, Notes.D3)]),
            BuildMeasure([Chord(Notes.A4, Notes.D3), Chord(Notes.A4, Notes.D3), Chord(Notes.A4, Notes.D3), Chord(Notes.A4, Notes.D3)]));

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert - structurally ineligible sites must not consume probability draws
        AllNotes(composition).Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.None);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplySuspensions_WhenEitherNoteIsOrnamented_ReplacesTheOrnamentWithTheSuspension()
    {
        // arrange - the preparation carries a passing tone; the structural suspension takes precedence over
        // the surface ornament, exactly as the cadential trill does at cadences
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.D3), Chord(Notes.E4, Notes.C3), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        var preparation = composition.Measures[0].Beats[1].Chord[Instrument.One];

        preparation.OrnamentationType = OrnamentationType.PassingTone;
        preparation.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Quarter));

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert - the passing tone is gone and the full figure is in place
        preparation.OrnamentationType.Should().Be(OrnamentationType.Suspension);
        preparation.Ornamentations.Should().BeEmpty();
        preparation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Half + MusicalTimeSpan.Quarter);
        composition.Measures[0].Beats[2].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.SuspensionResolution);
    }

    [Test]
    public void ApplySuspensions_WhenEitherNoteCarriesATrill_LeavesTheTrillAlone()
    {
        // arrange - a deliberately placed cadential trill must never be displaced by a suspension
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.D3), Chord(Notes.E4, Notes.C3), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        var resolution = composition.Measures[0].Beats[2].Chord[Instrument.One];

        resolution.OrnamentationType = OrnamentationType.Trill;

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert
        composition.Measures[0].Beats[1].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.None);
        resolution.OrnamentationType.Should().Be(OrnamentationType.Trill);
    }

    [Test]
    public void ApplySuspensions_DrawsProbabilityOnlyForEligibleSitesAndWithTheConfiguredWeight()
    {
        // arrange - the pinned bass leaves exactly one eligible site (the soprano's 4-3), so exactly one
        // probability draw may occur, and it must carry the configured probability
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.D3), Chord(Notes.F4, Notes.D3), Chord(Notes.E4, Notes.D3), Chord(Notes.G4, Notes.D3)]),
            BuildMeasure([Chord(Notes.A4, Notes.D3), Chord(Notes.A4, Notes.D3), Chord(Notes.A4, Notes.D3), Chord(Notes.A4, Notes.D3)]));

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(SuspensionConfiguration.Default.Probability);
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplySuspensions_WhenTheProbabilityDrawFails_DoesNotSuspend()
    {
        // arrange
        _mockWeightedRandomBooleanGenerator.IsTrue(Arg.Any<int>()).Returns(false);

        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.D3), Chord(Notes.E4, Notes.C3), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert
        AllNotes(composition).Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.None);
    }

    [Test]
    public void ApplySuspensions_WhenDisabled_DoesNothing()
    {
        // arrange
        var configuration = _compositionConfiguration with { SuspensionConfiguration = new SuspensionConfiguration(Enabled: false, Probability: 100) };

        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.D3), Chord(Notes.E4, Notes.C3), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        // act
        new SuspensionApplicator(_mockWeightedRandomBooleanGenerator, configuration).ApplySuspensions(composition);

        // assert
        AllNotes(composition).Should().OnlyContain(static note => note.OrnamentationType == OrnamentationType.None);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplySuspensions_WhenAVoiceIsMissingFromEitherChord_SkipsThatVoiceSafely()
    {
        // arrange - Instrument.Two is absent from the resolution chord; Instrument.One is not a step down
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.D3), new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)])), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        // act
        var act = () => CreateApplicator().ApplySuspensions(composition);

        // assert - Instrument.One still suspends; Instrument.Two is skipped without throwing
        act.Should().NotThrow();
        composition.Measures[0].Beats[1].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.Suspension);
        composition.Measures[0].Beats[1].Chord[Instrument.Two].OrnamentationType.Should().Be(OrnamentationType.None);
    }

    [Test]
    public void ApplySuspensions_WithAThreeBeatInteriorMeasure_NeverRestampsTheAliasedResolution()
    {
        // arrange - the ending composer can leave a three-beat leftover measure mid-composition, where the
        // cross-barline pair's preparation IS the mid-measure pair's just-stamped resolution. Both pairs are
        // harmonically eligible here: F4 -> E4 within the measure, and E4 -> D4 across the barline.
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.F3), Chord(Notes.E4, Notes.C3)]),
            BuildMeasure([Chord(Notes.D4, Notes.B2), Chord(Notes.G4, Notes.E3), Chord(Notes.G4, Notes.E3), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        var aliasedNote = composition.Measures[1].Beats[2].Chord[Instrument.One];

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert - the aliased note stays a resolution; the cross-barline pair is skipped, so the next
        // downbeat is untouched and the three-beat measure still spans exactly three beats for the voice
        composition.Measures[1].Beats[1].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.Suspension);
        aliasedNote.OrnamentationType.Should().Be(OrnamentationType.SuspensionResolution);
        aliasedNote.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
        composition.Measures[2].Beats[0].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.None);
    }

    [Test]
    public void ApplySuspensions_WithASingleBeatInteriorMeasure_NeverRestampsTheAliasedResolution()
    {
        // arrange - a single-beat interior measure's only note can be stamped as a resolution by the previous
        // measure's cross-barline pair and is then re-examined as the preparation of its own cross-barline
        // pair: F4 resolves to E4 on the single beat, which would itself be eligible to suspend into D4.
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.F3)]),
            BuildMeasure([Chord(Notes.E4, Notes.C3)]),
            BuildMeasure([Chord(Notes.D4, Notes.B2), Chord(Notes.G4, Notes.E3), Chord(Notes.G4, Notes.E3), Chord(Notes.G4, Notes.E3)]),
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3), Chord(Notes.A4, Notes.F3)]));

        var aliasedNote = composition.Measures[1].Beats[0].Chord[Instrument.One];

        // act
        CreateApplicator().ApplySuspensions(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.Suspension);
        aliasedNote.OrnamentationType.Should().Be(OrnamentationType.SuspensionResolution);
        aliasedNote.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
        composition.Measures[2].Beats[0].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.None);
    }

    [Test]
    public void ApplySuspensions_WithAShortTrailingMeasure_SkipsItSafely()
    {
        // arrange - the ending composer can emit a short final measure from leftover beats
        var composition = BuildComposition(
            BuildMeasure([Chord(Notes.A4, Notes.F3), Chord(Notes.F4, Notes.D3), Chord(Notes.E4, Notes.C3), Chord(Notes.G4, Notes.E3)]),
            new Measure(
                [Chord(Notes.A4, Notes.F3)],
                Meter.FourFour));

        // act
        var act = () => CreateApplicator().ApplySuspensions(composition);

        // assert - the mid-measure site in measure 0 still fires; the short final measure is untouched
        act.Should().NotThrow();
        composition.Measures[0].Beats[1].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.Suspension);
    }

    [Test]
    public void ApplySuspensions_InThreeFourTime_UsesTheMeterDefaultSpans()
    {
        // arrange - in 3/4 the default note span is a dotted half, so the tie is a dotted half plus its half.
        // DefaultNoteTimeSpan is an explicit constructor parameter, so it must be set alongside the meter.
        var configuration = TestCompositionConfigurations.Get(2) with
        {
            Meter = Meter.ThreeFour,
            DefaultNoteTimeSpan = Meter.ThreeFour.DefaultMusicalTimeSpan()
        };

        var composition = BuildComposition(
            BuildMeasure(
                [Chord(Notes.A4, Notes.F3, configuration), Chord(Notes.F4, Notes.D3, configuration), Chord(Notes.E4, Notes.C3, configuration), Chord(Notes.G4, Notes.E3, configuration)],
                Meter.ThreeFour),
            BuildMeasure(
                [Chord(Notes.A4, Notes.F3, configuration), Chord(Notes.A4, Notes.F3, configuration), Chord(Notes.A4, Notes.F3, configuration), Chord(Notes.A4, Notes.F3, configuration)],
                Meter.ThreeFour));

        var preparation = composition.Measures[0].Beats[1].Chord[Instrument.One];
        var resolution = composition.Measures[0].Beats[2].Chord[Instrument.One];

        // act
        new SuspensionApplicator(_mockWeightedRandomBooleanGenerator, configuration).ApplySuspensions(composition);

        // assert
        var defaultSpan = configuration.DefaultNoteTimeSpan;

        preparation.MusicalTimeSpan.Should().Be(defaultSpan + (defaultSpan / 2));
        resolution.MusicalTimeSpan.Should().Be(defaultSpan / 2);
    }

    private SuspensionApplicator CreateApplicator() => new(_mockWeightedRandomBooleanGenerator, _compositionConfiguration);

    private static Composition BuildComposition(params Measure[] measures) => new([.. measures]);

    private static Measure BuildMeasure(List<Beat> beats, Meter meter = Meter.FourFour) => new(beats, meter);

    private static Beat Chord(Note sopranoNote, Note altoNote) => new(new BaroquenChord([
        new BaroquenNote(Instrument.One, sopranoNote, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, altoNote, MusicalTimeSpan.Half)
    ]));

    private static Beat Chord(Note sopranoNote, Note altoNote, CompositionConfiguration configuration) => new(new BaroquenChord([
        new BaroquenNote(Instrument.One, sopranoNote, configuration.DefaultNoteTimeSpan),
        new BaroquenNote(Instrument.Two, altoNote, configuration.DefaultNoteTimeSpan)
    ]));

    private static IEnumerable<BaroquenNote> AllNotes(Composition composition) =>
        composition.Measures.SelectMany(static measure => measure.Beats).SelectMany(static beat => beat.Chord.Notes);
}
