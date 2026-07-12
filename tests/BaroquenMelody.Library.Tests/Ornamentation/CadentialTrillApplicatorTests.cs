using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Ornamentation;

[TestFixture]
internal sealed class CadentialTrillApplicatorTests
{
    [Test]
    public void ApplyTrill_AtAPerfectAuthenticCadence_TrillsTheLeadingToneVoice()
    {
        // arrange - the leading tone (B) sounds in Instrument.Three of the cadential dominant
        var compositionConfiguration = TestCompositionConfigurations.Get();
        var cadentialTrillApplicator = CreateCadentialTrillApplicator(compositionConfiguration);
        var penultimateChord = CreateRootPositionV();
        var finalChord = CreateRootPositionIWithTonicSoprano();

        // act
        cadentialTrillApplicator.ApplyTrill(penultimateChord, finalChord);

        // assert
        var trilledNote = penultimateChord[Instrument.Three];

        trilledNote.OrnamentationType.Should().Be(OrnamentationType.Trill);
        trilledNote.Ornamentations.Should().HaveCount(7);
        trilledNote.Ornamentations.Select(static note => note.Raw)
            .Should().Equal(Notes.C3, Notes.B2, Notes.C3, Notes.B2, Notes.C3, Notes.A2, Notes.B2);

        penultimateChord[Instrument.One].OrnamentationType.Should().Be(OrnamentationType.None);
        penultimateChord[Instrument.Two].OrnamentationType.Should().Be(OrnamentationType.None);
        penultimateChord[Instrument.Four].OrnamentationType.Should().Be(OrnamentationType.None);
    }

    [Test]
    public void ApplyTrill_AtAnImperfectAuthenticCadence_TrillsTheLeadingToneVoice()
    {
        // arrange - the dominant is inverted (leading tone in the bass), making the cadence imperfect
        var compositionConfiguration = TestCompositionConfigurations.Get();
        var cadentialTrillApplicator = CreateCadentialTrillApplicator(compositionConfiguration);

        var penultimateChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.B1, MusicalTimeSpan.Half)
        ]);

        var finalChord = CreateRootPositionIWithTonicSoprano();

        // act
        cadentialTrillApplicator.ApplyTrill(penultimateChord, finalChord);

        // assert
        penultimateChord[Instrument.Four].OrnamentationType.Should().Be(OrnamentationType.Trill);
    }

    [Test]
    public void ApplyTrill_WhenTheCadenceIsNotAuthentic_DoesNotTrill()
    {
        // arrange - I moving to V is a half cadence, not an authentic one
        var compositionConfiguration = TestCompositionConfigurations.Get();
        var cadentialTrillApplicator = CreateCadentialTrillApplicator(compositionConfiguration);
        var penultimateChord = CreateRootPositionIWithTonicSoprano();
        var finalChord = CreateRootPositionV();

        // act
        cadentialTrillApplicator.ApplyTrill(penultimateChord, finalChord);

        // assert
        AssertNoNoteIsTrilled(penultimateChord);
    }

    [Test]
    public void ApplyTrill_WhenTheTrillOrnamentationIsDisabled_DoesNotTrill()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get() with
        {
            AggregateOrnamentationConfiguration = new AggregateOrnamentationConfiguration(
                new HashSet<OrnamentationConfiguration>
                {
                    new(OrnamentationType.Trill, ConfigurationStatus.Disabled, Probability: 20)
                })
        };

        var cadentialTrillApplicator = CreateCadentialTrillApplicator(compositionConfiguration);
        var penultimateChord = CreateRootPositionV();
        var finalChord = CreateRootPositionIWithTonicSoprano();

        // act
        cadentialTrillApplicator.ApplyTrill(penultimateChord, finalChord);

        // assert
        AssertNoNoteIsTrilled(penultimateChord);
    }

    [Test]
    public void ApplyTrill_WhenTheTrillOrnamentationIsNotConfigured_DoesNotTrill()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get() with
        {
            AggregateOrnamentationConfiguration = new AggregateOrnamentationConfiguration(
                new HashSet<OrnamentationConfiguration>
                {
                    new(OrnamentationType.Mordent, ConfigurationStatus.Enabled, Probability: 20)
                })
        };

        var cadentialTrillApplicator = CreateCadentialTrillApplicator(compositionConfiguration);
        var penultimateChord = CreateRootPositionV();
        var finalChord = CreateRootPositionIWithTonicSoprano();

        // act
        cadentialTrillApplicator.ApplyTrill(penultimateChord, finalChord);

        // assert
        AssertNoNoteIsTrilled(penultimateChord);
    }

    [Test]
    public void ApplyTrill_WhenNoVoiceSoundsTheLeadingTone_DoesNotTrill()
    {
        // arrange - a partially voiced dominant without its third still classifies as V, but has nothing to trill
        var compositionConfiguration = TestCompositionConfigurations.Get();
        var cadentialTrillApplicator = CreateCadentialTrillApplicator(compositionConfiguration);

        var penultimateChord = new BaroquenChord([
            new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half),
            new BaroquenNote(Instrument.Four, Notes.G1, MusicalTimeSpan.Half)
        ]);

        var finalChord = CreateRootPositionIWithTonicSoprano();

        // act
        cadentialTrillApplicator.ApplyTrill(penultimateChord, finalChord);

        // assert
        AssertNoNoteIsTrilled(penultimateChord);
    }

    [Test]
    public void ApplyTrill_WhenTheUpperNeighborLeavesTheInstrumentRange_DoesNotTrill()
    {
        // arrange - the leading-tone voice tops out exactly on the leading tone, so the trill's upper neighbor
        // would leave its range
        var compositionConfiguration = CreateConfigurationWithInstrumentTwoRange(Notes.D3, Notes.B3);
        var cadentialTrillApplicator = CreateCadentialTrillApplicator(compositionConfiguration);
        var penultimateChord = CreateVWithLeadingToneInInstrumentTwo();
        var finalChord = CreateRootPositionIWithTonicSoprano();

        // act
        cadentialTrillApplicator.ApplyTrill(penultimateChord, finalChord);

        // assert
        AssertNoNoteIsTrilled(penultimateChord);
    }

    [Test]
    public void ApplyTrill_WhenTheLowerNeighborLeavesTheInstrumentRange_DoesNotTrill()
    {
        // arrange - the leading-tone voice bottoms out exactly on the leading tone, so the trill's closing
        // lower neighbor would leave its range
        var compositionConfiguration = CreateConfigurationWithInstrumentTwoRange(Notes.B3, Notes.B4);
        var cadentialTrillApplicator = CreateCadentialTrillApplicator(compositionConfiguration);
        var penultimateChord = CreateVWithLeadingToneInInstrumentTwo();
        var finalChord = CreateRootPositionIWithTonicSoprano();

        // act
        cadentialTrillApplicator.ApplyTrill(penultimateChord, finalChord);

        // assert
        AssertNoNoteIsTrilled(penultimateChord);
    }

    private static CadentialTrillApplicator CreateCadentialTrillApplicator(CompositionConfiguration compositionConfiguration)
    {
        var chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);

        return new CadentialTrillApplicator(
            new CadenceClassifier(
                chordNumberIdentifier,
                new ChordInversionIdentifier(chordNumberIdentifier, compositionConfiguration),
                compositionConfiguration),
            new OrnamentationProcessorConfigurationFactory(
                chordNumberIdentifier,
                new WeightedRandomBooleanGenerator(new ThreadLocalRandomProvider()),
                compositionConfiguration,
                Substitute.For<ILogger>()),
            new MusicalTimeSpanCalculator(),
            compositionConfiguration);
    }

    // Constructed from scratch rather than via a `with` expression: the configuration's register-ordered
    // Instruments and per-instrument range lookups are computed in property initializers, which do not re-run
    // for non-destructive mutation.
    private static CompositionConfiguration CreateConfigurationWithInstrumentTwoRange(Note minNote, Note maxNote) => new(
        new HashSet<InstrumentConfiguration>
        {
            new(Instrument.One, Notes.C4, Notes.C6, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Two, minNote, maxNote, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Three, Notes.C2, Notes.C3, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
            new(Instrument.Four, Notes.C1, Notes.C2, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
        },
        PhrasingConfiguration.Default,
        AggregateCompositionRuleConfiguration.Default,
        AggregateOrnamentationConfiguration.Default,
        NoteName.C,
        Mode.Ionian,
        Meter.FourFour,
        MusicalTimeSpan.Half,
        MinimumMeasures: 100);

    private static BaroquenChord CreateRootPositionV() => new([
        new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, Notes.D3, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Three, Notes.B2, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Four, Notes.G1, MusicalTimeSpan.Half)
    ]);

    private static BaroquenChord CreateVWithLeadingToneInInstrumentTwo() => new([
        new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, Notes.B3, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Three, Notes.D2, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Four, Notes.G1, MusicalTimeSpan.Half)
    ]);

    private static BaroquenChord CreateRootPositionIWithTonicSoprano() => new([
        new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half),
        new BaroquenNote(Instrument.Four, Notes.C2, MusicalTimeSpan.Half)
    ]);

    private static void AssertNoNoteIsTrilled(BaroquenChord chord)
    {
        foreach (var note in chord.Notes)
        {
            note.OrnamentationType.Should().Be(OrnamentationType.None);
            note.Ornamentations.Should().BeEmpty();
        }
    }
}
