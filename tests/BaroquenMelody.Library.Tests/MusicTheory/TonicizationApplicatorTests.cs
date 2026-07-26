using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NSubstitute;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.MusicTheory;

/// <summary>
///     The chords below are voiced so the first-match subset classifier reads the intended numeral in
///     A Aeolian: {G,B} reads as v, {A,C} and {C,E} as i, {D,A} as iv, {G,B,D} as VII, {C,E,G} as III.
/// </summary>
[TestFixture]
internal sealed class TonicizationApplicatorTests
{
    private IWeightedRandomBooleanGenerator _mockWeightedRandomBooleanGenerator = null!;

    private CompositionConfiguration _compositionConfiguration = null!;

    [SetUp]
    public void SetUp()
    {
        _mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();
        _mockWeightedRandomBooleanGenerator.IsTrue(Arg.Any<int>()).Returns(true);

        _compositionConfiguration = TestCompositionConfigurations.Get(2, tonic: NoteName.A, mode: Mode.Aeolian);
    }

    [Test]
    public void ApplyTonicization_AtTheClosingCadence_RaisesTheThirdOfVIntoTheFinalMeasure()
    {
        // arrange - the flagship: v approaching i across the final barline; unlike suspensions, the pair
        // into the final measure is included, and only the approaching chord is altered
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert - the soprano's G4 becomes G#4, the raised leading tone; the bass B is not the third
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.GSharp4);
        composition.Measures[0].Beats[^1].Chord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.None);
        composition.Measures[0].Beats[^1].Chord[Instrument.Two].Raw.Should().Be(Notes.B2);
        composition.Measures[1].Beats[0].Chord[Instrument.One].Raw.Should().Be(Notes.A4);
    }

    [Test]
    public void ApplyTonicization_AtAMidMeasureHarmonicChange_RaisesTheThird()
    {
        // arrange - v on slot 1 approaching i on the strong slot 2
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[1].Chord[Instrument.One].Raw.Should().Be(Notes.GSharp4);
    }

    [Test]
    public void ApplyTonicization_WhenTheDominantApproachesTheSubdominant_RaisesTheTonicChordsThird()
    {
        // arrange - i approaching iv: the tonic chord becomes V/iv by raising its third C to C#
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.E4, Notes.A2), Chord(Notes.E4, Notes.A2), Chord(Notes.E4, Notes.A2), Chord(Notes.C5, Notes.E3)),
            BuildMeasure(Chord(Notes.D5, Notes.A2), Chord(Notes.D5, Notes.A2), Chord(Notes.D5, Notes.A2), Chord(Notes.D5, Notes.A2)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.CSharp5);
        composition.Measures[0].Beats[^1].Chord[Instrument.Two].Raw.Should().Be(Notes.E3);
    }

    [Test]
    public void ApplyTonicization_WhenTheThirdIsDoubled_RaisesEveryDoubling()
    {
        // arrange - three voices, the third G doubled in soprano and bass, both resolving to A
        var configuration = TestCompositionConfigurations.Get(3, tonic: NoteName.A, mode: Mode.Aeolian);
        var composition = BuildComposition(
            BuildMeasure(
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.A2),
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.A2),
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.A2),
                ThreeVoiceChord(Notes.G4, Notes.B3, Notes.G2)),
            BuildMeasure(
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.A2),
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.A2),
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.A2),
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.A2)));

        // act
        new TonicizationApplicator(new ChordNumberIdentifier(configuration), _mockWeightedRandomBooleanGenerator, configuration).ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.GSharp4);
        composition.Measures[0].Beats[^1].Chord[Instrument.Three].Raw.Should().Be(Notes.GSharp2);
        composition.Measures[0].Beats[^1].Chord[Instrument.Two].Raw.Should().Be(Notes.B3);
    }

    [Test]
    public void ApplyTonicization_WhenOneDoublingFailsItsResolutionObligation_RaisesNothing()
    {
        // arrange - the bass G resolves to E instead of A, so the whole site must be rejected: a partial
        // raise would sound G and G# together
        var configuration = TestCompositionConfigurations.Get(3, tonic: NoteName.A, mode: Mode.Aeolian);
        var composition = BuildComposition(
            BuildMeasure(
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.A2),
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.A2),
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.A2),
                ThreeVoiceChord(Notes.G4, Notes.B3, Notes.G2)),
            BuildMeasure(
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.E2),
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.E2),
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.E2),
                ThreeVoiceChord(Notes.A4, Notes.C4, Notes.E2)));

        // act
        new TonicizationApplicator(new ChordNumberIdentifier(configuration), _mockWeightedRandomBooleanGenerator, configuration).ApplyTonicization(composition);

        // assert
        AllNotes(composition).Should().OnlyContain(static note => note.Raw.NoteName != NoteName.GSharp);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenAParticipantIsInsideASuspensionFigure_RaisesNothing()
    {
        // arrange - the third carries a suspension stamp; restamping or partially contradicting the
        // figure is forbidden, so the site is rejected
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        composition.Measures[0].Beats[^1].Chord[Instrument.One].OrnamentationType = OrnamentationType.Suspension;

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenTheSourceTriadIsNotMinor_RaisesNothing()
    {
        // arrange - VII (G major) approaching III ({C,E,G}, voiced to dodge the subset classifier's
        // earlier numerals) is a fifth-above pair, but only minor triads license a raise: the major VII
        // already is a dominant
        var configuration = TestCompositionConfigurations.Get(3, tonic: NoteName.A, mode: Mode.Aeolian);
        var composition = BuildComposition(
            BuildMeasure(
                ThreeVoiceChord(Notes.C5, Notes.E4, Notes.G2),
                ThreeVoiceChord(Notes.C5, Notes.E4, Notes.G2),
                ThreeVoiceChord(Notes.C5, Notes.E4, Notes.G2),
                ThreeVoiceChord(Notes.G4, Notes.B3, Notes.D2)),
            BuildMeasure(
                ThreeVoiceChord(Notes.C5, Notes.E4, Notes.G2),
                ThreeVoiceChord(Notes.C5, Notes.E4, Notes.G2),
                ThreeVoiceChord(Notes.C5, Notes.E4, Notes.G2),
                ThreeVoiceChord(Notes.C5, Notes.E4, Notes.G2)));

        // act
        new TonicizationApplicator(new ChordNumberIdentifier(configuration), _mockWeightedRandomBooleanGenerator, configuration).ApplyTonicization(composition);

        // assert - VII's third B stays natural and no probability is ever drawn
        composition.Measures[0].Beats[^1].Chord[Instrument.Two].Raw.Should().Be(Notes.B3);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenTheSourceTriadIsDiminished_RaisesNothing()
    {
        // arrange - ii° ({B,D}) approaching v ({E,B}) is a true fifth-above pair whose third D would
        // resolve cleanly up to E, but raising a diminished triad's third alone leaves the B-F tritone
        // unresolved, so only minor triads license a raise
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.B4, Notes.D3)),
            BuildMeasure(Chord(Notes.B4, Notes.E3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.Two].Raw.Should().Be(Notes.D3);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenThePairIsNotAFifthApart_RaisesNothing()
    {
        // arrange - v resolving deceptively to VI is not a tonicization
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.F4, Notes.A2), Chord(Notes.F4, Notes.A2), Chord(Notes.F4, Notes.A2), Chord(Notes.F4, Notes.A2)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_OutsideAeolian_RaisesNothing()
    {
        // arrange - the same fifth-above shape in C Ionian; v1's license table is Aeolian-only
        var configuration = TestCompositionConfigurations.Get(2, tonic: NoteName.C, mode: Mode.Ionian);
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.C5, Notes.E3), Chord(Notes.C5, Notes.E3), Chord(Notes.C5, Notes.E3), Chord(Notes.B4, Notes.G3)),
            BuildMeasure(Chord(Notes.C5, Notes.E3), Chord(Notes.C5, Notes.E3), Chord(Notes.C5, Notes.E3), Chord(Notes.C5, Notes.E3)));

        // act
        new TonicizationApplicator(new ChordNumberIdentifier(configuration), _mockWeightedRandomBooleanGenerator, configuration).ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.B4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenTheRaisedNoteWouldLeaveTheInstrumentRange_RaisesNothing()
    {
        // arrange - the third sits on the bass's highest playable note (G4), so even though it steps up
        // cleanly into A4, the raised G#4 itself would be out of range
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.B4, Notes.G4)),
            BuildMeasure(Chord(Notes.A4, Notes.A4), Chord(Notes.A4, Notes.A4), Chord(Notes.A4, Notes.A4), Chord(Notes.A4, Notes.A4)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.Two].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenTheParticipantsVoiceIsMissingFromTheTarget_RaisesNothing()
    {
        // arrange - the soprano holds the third but does not continue into the target beat
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(new Beat(new BaroquenChord([new BaroquenNote(Instrument.Two, Notes.A2, MusicalTimeSpan.Half)])), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenTheSourceChordLacksItsThird_RaisesNothing()
    {
        // arrange - {E,B} still reads as v, but there is no third to raise
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.B4, Notes.E3)),
            BuildMeasure(Chord(Notes.A4, Notes.A2), Chord(Notes.A4, Notes.A2), Chord(Notes.A4, Notes.A2), Chord(Notes.A4, Notes.A2)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        AllNotes(composition).Should().OnlyContain(static note => note.Raw.NoteName != NoteName.GSharp);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_RaisesTheParticipantsOrnamentationWithIt()
    {
        // arrange - the raised voice's figures move with it: sub-notes on the third's pitch class are
        // raised, and the whole-step lower neighbor F is raised too (the melodic-minor courtesy that
        // keeps the figure free of the F-G# augmented second); other sub-notes stay diatonic
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        var participant = composition.Measures[0].Beats[^1].Chord[Instrument.One];

        participant.OrnamentationType = OrnamentationType.Turn;
        participant.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Eighth));
        participant.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Eighth));
        participant.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        participant.Raw.Should().Be(Notes.GSharp4);
        participant.OrnamentationType.Should().Be(OrnamentationType.Turn);
        participant.Ornamentations[0].Raw.Should().Be(Notes.A4);
        participant.Ornamentations[1].Raw.Should().Be(Notes.FSharp4);
        participant.Ornamentations[2].Raw.Should().Be(Notes.GSharp4);
    }

    [Test]
    public void ApplyTonicization_DoesNotApplyTheCourtesyAcrossAHalfStep()
    {
        // arrange - i to iv raises C, whose lower neighbor B sits a half step below: B-C# is already a
        // clean whole step, so B stays natural
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.E4, Notes.A2), Chord(Notes.E4, Notes.A2), Chord(Notes.E4, Notes.A2), Chord(Notes.C5, Notes.E3)),
            BuildMeasure(Chord(Notes.D5, Notes.A2), Chord(Notes.D5, Notes.A2), Chord(Notes.D5, Notes.A2), Chord(Notes.D5, Notes.A2)));

        var participant = composition.Measures[0].Beats[^1].Chord[Instrument.One];

        participant.OrnamentationType = OrnamentationType.Mordent;
        participant.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Eighth));
        participant.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        participant.Raw.Should().Be(Notes.CSharp5);
        participant.Ornamentations[0].Raw.Should().Be(Notes.B4);
        participant.Ornamentations[1].Raw.Should().Be(Notes.CSharp5);
    }

    [Test]
    public void ApplyTonicization_RaisesEveryVoicesFigureThatSoundsTheThird()
    {
        // arrange - the bass B carries a run that sounds G natural during the raised chord; the figure
        // is a participant in the tonicization, not a casualty: its G raises with the harmony in its own
        // octave and the run survives intact, leaving no natural third to sound as a false relation
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        var bystander = composition.Measures[0].Beats[^1].Chord[Instrument.Two];

        bystander.OrnamentationType = OrnamentationType.Run;
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.A2, MusicalTimeSpan.Eighth));
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.G2, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.GSharp4);
        bystander.OrnamentationType.Should().Be(OrnamentationType.Run);
        bystander.Ornamentations[0].Raw.Should().Be(Notes.A2);
        bystander.Ornamentations[1].Raw.Should().Be(Notes.GSharp2);
    }

    [Test]
    public void ApplyTonicization_LeavesABystandersUnrelatedOrnamentAlone()
    {
        // arrange - the bass's figure never sounds the natural third, so it keeps its decoration
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        var bystander = composition.Measures[0].Beats[^1].Chord[Instrument.Two];

        bystander.OrnamentationType = OrnamentationType.NeighborTone;
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        bystander.OrnamentationType.Should().Be(OrnamentationType.NeighborTone);
        bystander.Ornamentations.Should().HaveCount(1);
    }

    [Test]
    public void ApplyTonicization_WhenDisabled_DoesNothing()
    {
        // arrange
        var configuration = _compositionConfiguration with { TonicizationConfiguration = new TonicizationConfiguration(Enabled: false, Probability: 0) };
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        // act
        new TonicizationApplicator(new ChordNumberIdentifier(configuration), _mockWeightedRandomBooleanGenerator, configuration).ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_DrawsProbabilityOnceForASingleEligibleSite()
    {
        // arrange - one eligible site; the draw happens only after every structural guard passes
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(TonicizationConfiguration.Default.Probability);
    }

    [Test]
    public void ApplyTonicization_WhenTheProbabilityDrawFails_RaisesNothing()
    {
        // arrange - a fully eligible site whose single draw comes up false
        _mockWeightedRandomBooleanGenerator.IsTrue(Arg.Any<int>()).Returns(false);

        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.Received(1).IsTrue(TonicizationConfiguration.Default.Probability);
    }

    [Test]
    public void ApplyTonicization_InsideTheFinalMeasure_RaisesNothing()
    {
        // arrange - the final measure holds an eligible mid-measure pair, but the final measure is never
        // a dominant: its crafted cadence stays unaltered, and in the exposition pass this same rule
        // keeps the boundary measure borrowed from the already-tonicized body from being drawn twice
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[1].Beats[1].Chord[Instrument.One].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenAMeasureHasOnlyTwoBeats_StillAppliesTheCrossBarlinePair()
    {
        // arrange - the short leftover measures the ending composer can produce have no mid-measure
        // strong slot, but their final beat still approaches the next downbeat
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.GSharp4);
    }

    [Test]
    public void ApplyTonicization_WhenTheTargetChordIsUnidentifiable_RaisesNothing()
    {
        // arrange - {A,B} matches no diatonic triad, so the target reads as Unknown and rejects through
        // the dominant lookup
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.B2), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenTheResolutionArrivesInAnotherOctave_RaisesNothing()
    {
        // arrange - the soprano lands on the target's root pitch class but an octave too high: a raised
        // leading tone must resolve up by half step to the root directly above it, so the site is
        // rejected on register, not just pitch class
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A5, Notes.A2), Chord(Notes.A5, Notes.A2), Chord(Notes.A5, Notes.A2), Chord(Notes.A5, Notes.A2)));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_RaisesTheCourtesyToneSteppingUpIntoARaisedThird()
    {
        // arrange - the bass's run steps F-G straight into the raise: leaving the F natural would sound
        // the F-G# augmented second inside the figure, so it rises with the third into the classic
        // ascending-melodic-minor close, and the whole figure survives
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        var bystander = composition.Measures[0].Beats[^1].Chord[Instrument.Two];

        bystander.OrnamentationType = OrnamentationType.Run;
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Eighth));
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.GSharp4);
        bystander.OrnamentationType.Should().Be(OrnamentationType.Run);
        bystander.Ornamentations[0].Raw.Should().Be(Notes.FSharp3);
        bystander.Ornamentations[1].Raw.Should().Be(Notes.GSharp3);
    }

    [Test]
    public void ApplyTonicization_RaisesTheCourtesyToneSteppingDownFromARaisedThird()
    {
        // arrange - the augmented second is just as jarring descending: a figure falling G-F from the
        // raised third takes the courtesy too, G#-F#
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        var bystander = composition.Measures[0].Beats[^1].Chord[Instrument.Two];

        bystander.OrnamentationType = OrnamentationType.Run;
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Eighth));
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        bystander.Ornamentations[0].Raw.Should().Be(Notes.GSharp3);
        bystander.Ornamentations[1].Raw.Should().Be(Notes.FSharp3);
    }

    [Test]
    public void ApplyTonicization_LeavesANonAdjacentCourtesyToneNatural()
    {
        // arrange - the bass's figure sounds F natural but never steps into a raised third, so there is
        // no augmented second to close and the natural sixth keeps its minor-mode color
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        var bystander = composition.Measures[0].Beats[^1].Chord[Instrument.Two];

        bystander.OrnamentationType = OrnamentationType.Run;
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.GSharp4);
        bystander.OrnamentationType.Should().Be(OrnamentationType.Run);
        bystander.Ornamentations[0].Raw.Should().Be(Notes.F3);
    }

    [Test]
    public void ApplyTonicization_WhenTheFigureRepeatsTheCourtesyTone_RaisesOnlyTheStepIntoTheThird()
    {
        // arrange - only the F that steps directly into the raised third takes the courtesy; the earlier
        // repetition keeps its natural, leaving the chromatic ascent F-F#-G#
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        var bystander = composition.Measures[0].Beats[^1].Chord[Instrument.Two];

        bystander.OrnamentationType = OrnamentationType.Run;
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Eighth));
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Eighth));
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        bystander.Ornamentations[0].Raw.Should().Be(Notes.F3);
        bystander.Ornamentations[1].Raw.Should().Be(Notes.FSharp3);
        bystander.Ornamentations[2].Raw.Should().Be(Notes.GSharp3);
    }

    [Test]
    public void ApplyTonicization_WhenAnyVoicesRaisedFigureWouldLeaveTheInstrumentRange_RaisesNothing()
    {
        // arrange - the bass is not a doubled third, but its figure sounds G on the bass's highest
        // playable note: its raise is as obligatory as any other, so the whole site is rejected and
        // every figure stays intact
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.G4, Notes.B2)),
            BuildMeasure(Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3), Chord(Notes.A4, Notes.C3)));

        var bystander = composition.Measures[0].Beats[^1].Chord[Instrument.Two];

        bystander.OrnamentationType = OrnamentationType.Run;
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.G4, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        composition.Measures[0].Beats[^1].Chord[Instrument.One].Raw.Should().Be(Notes.G4);
        bystander.Ornamentations[0].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenARaisedFigureWouldLeaveTheInstrumentRange_RaisesNothing()
    {
        // arrange - the bass third G3 raises cleanly, but its upper octave pedal sits on the bass's
        // highest playable note G4, so the raised figure would leave the range: the same all-or-nothing
        // obligation the doubled thirds carry rejects the whole site
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.B4, Notes.G3)),
            BuildMeasure(Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3)));

        var participant = composition.Measures[0].Beats[^1].Chord[Instrument.Two];

        participant.OrnamentationType = OrnamentationType.UpperOctavePedal;
        participant.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.G4, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        participant.Raw.Should().Be(Notes.G3);
        participant.Ornamentations[0].Raw.Should().Be(Notes.G4);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_WhenARaisedCourtesyToneWouldLeaveTheInstrumentRange_RaisesNothing()
    {
        // arrange - the bass third G3 raises cleanly, but its figure dips to the courtesy neighbor F3
        // just below the bass's lowest playable note G3: the raised F#3 would still sit outside the
        // range, so the whole site is rejected with the figure intact
        var configuration = BuildConfiguration(
            new InstrumentConfiguration(Instrument.One, Notes.C4, Notes.C6, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new InstrumentConfiguration(Instrument.Two, Notes.G3, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled));

        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.B4, Notes.G3)),
            BuildMeasure(Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3)));

        var participant = composition.Measures[0].Beats[^1].Chord[Instrument.Two];

        participant.OrnamentationType = OrnamentationType.Turn;
        participant.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Eighth));

        // act
        new TonicizationApplicator(new ChordNumberIdentifier(configuration), _mockWeightedRandomBooleanGenerator, configuration).ApplyTonicization(composition);

        // assert
        participant.Raw.Should().Be(Notes.G3);
        participant.Ornamentations[0].Raw.Should().Be(Notes.F3);
        _mockWeightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
    }

    [Test]
    public void ApplyTonicization_DoesNotApplyTheCourtesyAcrossALeapFromTheRaisedPrincipal()
    {
        // arrange - the bass's figure leaps from the raised G#3 up to F4: an augmented second only
        // exists between adjacent steps, so the leaped-to F keeps its natural
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.B4, Notes.G3)),
            BuildMeasure(Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3)));

        var participant = composition.Measures[0].Beats[^1].Chord[Instrument.Two];

        participant.OrnamentationType = OrnamentationType.Turn;
        participant.Ornamentations.Add(new BaroquenNote(Instrument.Two, Notes.F4, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        participant.Raw.Should().Be(Notes.GSharp3);
        participant.Ornamentations[0].Raw.Should().Be(Notes.F4);
    }

    [Test]
    public void ApplyTonicization_DoesNotApplyTheCourtesyAcrossALeapToTheRaisedThird()
    {
        // arrange - the soprano's figure sounds F4 and then leaps to a raised G#5 an octave above: no
        // augmented second exists across the leap, so the F keeps its natural
        var composition = BuildComposition(
            BuildMeasure(Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.B4, Notes.G3)),
            BuildMeasure(Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3), Chord(Notes.A4, Notes.A3)));

        var bystander = composition.Measures[0].Beats[^1].Chord[Instrument.One];

        bystander.OrnamentationType = OrnamentationType.Run;
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Eighth));
        bystander.Ornamentations.Add(new BaroquenNote(Instrument.One, Notes.G5, MusicalTimeSpan.Eighth));

        // act
        CreateApplicator().ApplyTonicization(composition);

        // assert
        bystander.Ornamentations[0].Raw.Should().Be(Notes.F4);
        bystander.Ornamentations[1].Raw.Should().Be(Notes.GSharp5);
    }

    private static Beat Chord(Note sopranoNote, Note bassNote) => new(new BaroquenChord(
    [
        new BaroquenNote(Instrument.One, sopranoNote, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, bassNote, MusicalTimeSpan.Half)
    ]));

    private static Beat ThreeVoiceChord(Note sopranoNote, Note altoNote, Note bassNote) => new(new BaroquenChord(
    [
        new BaroquenNote(Instrument.One, sopranoNote, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, altoNote, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Three, bassNote, MusicalTimeSpan.Half)
    ]));

    private static Measure BuildMeasure(params Beat[] beats) => new([.. beats], Meter.FourFour);

    private static Composition BuildComposition(params Measure[] measures) => new([.. measures]);

    private static CompositionConfiguration BuildConfiguration(params InstrumentConfiguration[] instrumentConfigurations) => new(
        instrumentConfigurations.ToHashSet(),
        PhrasingConfiguration.Default,
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        NoteName.A,
        Mode.Aeolian,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        MinimumMeasures: 2);

    private static IEnumerable<BaroquenNote> AllNotes(Composition composition) => composition.Measures
        .SelectMany(static measure => measure.Beats)
        .SelectMany(static beat => beat.Chord.Notes);

    private TonicizationApplicator CreateApplicator() => new(
        new ChordNumberIdentifier(_compositionConfiguration),
        _mockWeightedRandomBooleanGenerator,
        _compositionConfiguration);
}
