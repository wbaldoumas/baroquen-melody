using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Motifs;
using BaroquenMelody.Library.Motifs.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Motifs;

[TestFixture]
internal sealed class MotifDeveloperTests
{
    private const int Seed = 1234;

    [TestCase(MotifTransform.Invert)]
    [TestCase(MotifTransform.Retrograde)]
    [TestCase(MotifTransform.Augment)] // a reserved (unwired) transform must degrade to the grid-safe Identity, not an unsafe path.
    public void TryDevelop_DevelopsTheSelectedVoiceWithTheConfiguredTransform_ReAnchoredToThePhrase(MotifTransform transform)
    {
        // arrange: a single cataloged voice (Instrument.One) forces the voice draw, and a single-transform repertoire
        // forces the transform under test, so the only thing exercised is the re-anchor + apply + re-assembly math.
        var configuration = ConfigurationWith(new MotifDevelopmentConfiguration(true, [transform], 100, MotifDevelopmentScope.SingleVoice));
        var motifApplicator = new MotifApplicator(configuration);

        // the bank motif's own anchor is deliberately bogus (unused) — the developer must re-anchor to the phrase's head.
        var motif = new Motif([
            new MotivicGesture(0, MusicalTimeSpan.Half),
            new MotivicGesture(-1, MusicalTimeSpan.Half),
            new MotivicGesture(1, MusicalTimeSpan.Half),
            new MotivicGesture(-1, MusicalTimeSpan.Half)
        ]);

        var motifBank = new MotifBank(new Dictionary<Instrument, AnchoredMotif>
        {
            [Instrument.One] = new AnchoredMotif(motif, 0, Instrument.One)
        });

        // the phrase's first measure: Instrument.One enters on E4 (the re-anchor point); Instrument.Two is present but
        // uncataloged (so it can never be the developed voice) and must survive verbatim.
        var phrase = new List<Measure>
        {
            BuildMeasure(
                BuildChord(BuildNote(Instrument.One, Notes.E4), BuildNote(Instrument.Two, Notes.G3)),
                BuildChord(BuildNote(Instrument.One, Notes.F4), BuildNote(Instrument.Two, Notes.G3)),
                BuildChord(BuildNote(Instrument.One, Notes.G4), BuildNote(Instrument.Two, Notes.G3)),
                BuildChord(BuildNote(Instrument.One, Notes.A4), BuildNote(Instrument.Two, Notes.G3))
            )
        };

        // independently compute the developed line: the transform applied to the motif, re-anchored to the phrase's first
        // Instrument.One note. A reserved transform (Augment) must map to Identity, mirroring MotifDeveloper.ApplyTransform.
        var expectedMotif = transform switch
        {
            MotifTransform.Invert => MotifTransformations.Invert(motif),
            MotifTransform.Retrograde => MotifTransformations.Retrograde(motif),
            _ => MotifTransformations.Identity(motif)
        };

        var expectedNotes = motifApplicator.Apply(new AnchoredMotif(
            expectedMotif,
            configuration.Scale.IndexOf(BuildNote(Instrument.One, Notes.E4)),
            Instrument.One));

        var motifDeveloper = BuildMotifDeveloper(motifApplicator, configuration, new SeededRandomProvider(Seed));

        // act
        var developed = motifDeveloper.TryDevelop(motifBank, phrase);

        // assert
        developed.Should().NotBeNull();
        developed!.Should().HaveCount(1);

        var developedBeats = developed[0].Beats;
        developedBeats.Should().HaveCount(4);

        for (var beatIndex = 0; beatIndex < developedBeats.Count; ++beatIndex)
        {
            var developedNote = developedBeats[beatIndex][Instrument.One];

            developedNote.Raw.Should().Be(expectedNotes[beatIndex].Raw, "the developed voice is the configured transform re-anchored to the phrase");
            developedNote.MusicalTimeSpan.Should().Be(configuration.DefaultNoteTimeSpan, "developed notes are clean principal notes for the trailing ornamentation pass");
            developedNote.OrnamentationType.Should().Be(OrnamentationType.None);
            developedNote.Ornamentations.Should().BeEmpty();

            developedBeats[beatIndex][Instrument.Two].Raw.Should().Be(Notes.G3, "the uncataloged voice is repeated verbatim");
        }
    }

    [Test]
    public void TryDevelop_WhenTheAppliedLineDoesNotTileTheVoiceBeats_FallsBackToVerbatim()
    {
        // arrange: a four-gesture motif against a phrase where Instrument.One sounds on only two beats cannot tile 1:1.
        var configuration = ConfigurationWith(new MotifDevelopmentConfiguration(true, [MotifTransform.Invert], 100, MotifDevelopmentScope.SingleVoice));

        var motif = new Motif([
            new MotivicGesture(0, MusicalTimeSpan.Half),
            new MotivicGesture(-1, MusicalTimeSpan.Half),
            new MotivicGesture(1, MusicalTimeSpan.Half),
            new MotivicGesture(-1, MusicalTimeSpan.Half)
        ]);

        var motifBank = new MotifBank(new Dictionary<Instrument, AnchoredMotif>
        {
            [Instrument.One] = new AnchoredMotif(motif, 0, Instrument.One)
        });

        var phrase = new List<Measure>
        {
            BuildMeasure(
                BuildChord(BuildNote(Instrument.One, Notes.E4)),
                BuildChord(BuildNote(Instrument.One, Notes.F4))
            )
        };

        var motifDeveloper = BuildMotifDeveloper(new MotifApplicator(configuration), configuration, new SeededRandomProvider(Seed));

        // act
        var developed = motifDeveloper.TryDevelop(motifBank, phrase);

        // assert
        developed.Should().BeNull();
    }

    [Test]
    public void TryDevelop_SelectsTheCandidateVoiceFromTheOrderedInstrumentList()
    {
        // arrange: catalog BOTH voices (inserting in reverse instrument order), so a regression that iterated the bank's
        // frozen key order instead of the ordered instrument list could select a different voice.
        var configuration = ConfigurationWith(new MotifDevelopmentConfiguration(true, [MotifTransform.Invert], 100, MotifDevelopmentScope.SingleVoice));

        var motif = new Motif([new MotivicGesture(0, MusicalTimeSpan.Half), new MotivicGesture(-1, MusicalTimeSpan.Half)]);

        var motifBank = new MotifBank(new Dictionary<Instrument, AnchoredMotif>
        {
            [Instrument.Two] = new AnchoredMotif(motif, 0, Instrument.Two),
            [Instrument.One] = new AnchoredMotif(motif, 0, Instrument.One)
        });

        var phrase = new List<Measure>
        {
            BuildMeasure(
                BuildChord(BuildNote(Instrument.One, Notes.E4), BuildNote(Instrument.Two, Notes.G3)),
                BuildChord(BuildNote(Instrument.One, Notes.F4), BuildNote(Instrument.Two, Notes.A3))
            )
        };

        // with the voice draw pinned to index 0, the developed voice must be the FIRST candidate in instrument order.
        var expectedVoice = configuration.Instruments.First(motifBank.Contains);

        AnchoredMotif? developedAnchoredMotif = null;
        var motifApplicator = Substitute.For<IMotifApplicator>();
        motifApplicator
            .Apply(Arg.Do<AnchoredMotif>(anchoredMotif => developedAnchoredMotif = anchoredMotif))
            .Returns(callInfo => new List<BaroquenNote>
            {
                BuildNote(callInfo.Arg<AnchoredMotif>().Instrument, Notes.C4),
                BuildNote(callInfo.Arg<AnchoredMotif>().Instrument, Notes.C4)
            });

        var randomProvider = Substitute.For<IRandomProvider>();
        randomProvider.Next(Arg.Any<int>()).Returns(0);

        var weightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();
        weightedRandomBooleanGenerator.IsTrue(Arg.Any<int>()).Returns(true);

        var motifDeveloper = new MotifDeveloper(motifApplicator, weightedRandomBooleanGenerator, randomProvider, configuration);

        // act
        motifDeveloper.TryDevelop(motifBank, phrase);

        // assert
        developedAnchoredMotif.Should().NotBeNull();
        developedAnchoredMotif!.Instrument.Should().Be(expectedVoice, "the developed voice comes from the ordered instrument list, not the bank's key order");
    }

    [Test]
    public void TryDevelop_LeavesLaterMeasuresOfTheRestatementVerbatim()
    {
        // arrange: a two-measure phrase; the motif (+2 leap) inverts to a descending line, so the first measure visibly
        // develops while the tail measure must remain an untouched verbatim clone.
        var configuration = ConfigurationWith(new MotifDevelopmentConfiguration(true, [MotifTransform.Invert], 100, MotifDevelopmentScope.SingleVoice));

        var motif = new Motif([new MotivicGesture(0, MusicalTimeSpan.Half), new MotivicGesture(2, MusicalTimeSpan.Half)]);

        var motifBank = new MotifBank(new Dictionary<Instrument, AnchoredMotif>
        {
            [Instrument.One] = new AnchoredMotif(motif, 0, Instrument.One)
        });

        var phrase = new List<Measure>
        {
            BuildMeasure(BuildChord(BuildNote(Instrument.One, Notes.E4)), BuildChord(BuildNote(Instrument.One, Notes.F4))),
            BuildMeasure(BuildChord(BuildNote(Instrument.One, Notes.G4)), BuildChord(BuildNote(Instrument.One, Notes.A4)))
        };

        var motifDeveloper = BuildMotifDeveloper(new MotifApplicator(configuration), configuration, new SeededRandomProvider(Seed));

        // act
        var developed = motifDeveloper.TryDevelop(motifBank, phrase);

        // assert
        developed.Should().NotBeNull();
        developed!.Should().HaveCount(2);
        developed[0].Beats.Select(beat => beat[Instrument.One].Raw).Should().NotEqual([Notes.E4, Notes.F4], "the first measure is developed");
        developed[1].Beats.Select(beat => beat[Instrument.One].Raw).Should().Equal([Notes.G4, Notes.A4], "later measures of the phrase stay verbatim");
    }

    [Test]
    public void TryDevelop_WhenDevelopmentIsDisabled_ReturnsNullWithoutDrawingAnyRandomness()
    {
        // arrange
        var configuration = ConfigurationWith(new MotifDevelopmentConfiguration(false, [MotifTransform.Invert], 100, MotifDevelopmentScope.SingleVoice));
        var (motifBank, phrase) = SingleVoiceFixture();

        var weightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();
        var randomProvider = Substitute.For<IRandomProvider>();
        var motifDeveloper = new MotifDeveloper(Substitute.For<IMotifApplicator>(), weightedRandomBooleanGenerator, randomProvider, configuration);

        // act
        var developed = motifDeveloper.TryDevelop(motifBank, phrase);

        // assert: a disabled config must be a true no-op so the legacy random stream stays byte-identical.
        developed.Should().BeNull();
        weightedRandomBooleanGenerator.DidNotReceive().IsTrue(Arg.Any<int>());
        randomProvider.DidNotReceive().Next(Arg.Any<int>());
    }

    [Test]
    public void TryDevelop_WhenTheDevelopOrNotDrawIsFalse_ReturnsNullWithoutDrawingAVoiceOrTransform()
    {
        // arrange
        var configuration = ConfigurationWith(new MotifDevelopmentConfiguration(true, [MotifTransform.Invert], 50, MotifDevelopmentScope.SingleVoice));
        var (motifBank, phrase) = SingleVoiceFixture();

        var weightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();
        weightedRandomBooleanGenerator.IsTrue(Arg.Any<int>()).Returns(false);

        var randomProvider = Substitute.For<IRandomProvider>();
        var motifDeveloper = new MotifDeveloper(Substitute.For<IMotifApplicator>(), weightedRandomBooleanGenerator, randomProvider, configuration);

        // act
        var developed = motifDeveloper.TryDevelop(motifBank, phrase);

        // assert: the develop-or-not draw is spent (exactly once), but no voice/transform is drawn.
        developed.Should().BeNull();
        weightedRandomBooleanGenerator.Received(1).IsTrue(50);
        randomProvider.DidNotReceive().Next(Arg.Any<int>());
    }

    [Test]
    public void TryDevelop_WhenNoVoiceHasACatalogedMotif_ReturnsNull()
    {
        // arrange
        var configuration = ConfigurationWith(new MotifDevelopmentConfiguration(true, [MotifTransform.Invert], 100, MotifDevelopmentScope.SingleVoice));
        var (_, phrase) = SingleVoiceFixture();
        var emptyBank = new MotifBank(new Dictionary<Instrument, AnchoredMotif>());

        var weightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();
        weightedRandomBooleanGenerator.IsTrue(Arg.Any<int>()).Returns(true);

        var randomProvider = Substitute.For<IRandomProvider>();
        var motifDeveloper = new MotifDeveloper(Substitute.For<IMotifApplicator>(), weightedRandomBooleanGenerator, randomProvider, configuration);

        // act
        var developed = motifDeveloper.TryDevelop(emptyBank, phrase);

        // assert: the develop-or-not draw is spent (exactly once), then the empty candidate set short-circuits before any
        // voice/transform draw, keeping the draw count a strict function of (config, seed).
        developed.Should().BeNull();
        weightedRandomBooleanGenerator.Received(1).IsTrue(100);
        randomProvider.DidNotReceive().Next(Arg.Any<int>());
    }

    [Test]
    public void TryDevelop_WithAllVoicesScope_DevelopsEveryCatalogedVoice()
    {
        // Audit: suspension-tonicization-phrasing-motifs-7
        // arrange: both voices are cataloged and both move through the phrase; under AllVoices each developed line
        // must differ from its verbatim phrase line (Invert flips every step of both motifs).
        var configuration = ConfigurationWith(new MotifDevelopmentConfiguration(true, [MotifTransform.Invert], 100, MotifDevelopmentScope.AllVoices));
        var motifApplicator = new MotifApplicator(configuration);

        var motif = new Motif([
            new MotivicGesture(0, MusicalTimeSpan.Half),
            new MotivicGesture(1, MusicalTimeSpan.Half),
            new MotivicGesture(1, MusicalTimeSpan.Half),
            new MotivicGesture(1, MusicalTimeSpan.Half)
        ]);

        var motifBank = new MotifBank(new Dictionary<Instrument, AnchoredMotif>
        {
            [Instrument.One] = new AnchoredMotif(motif, 0, Instrument.One),
            [Instrument.Two] = new AnchoredMotif(motif, 0, Instrument.Two)
        });

        var phrase = new List<Measure>
        {
            BuildMeasure(
                BuildChord(BuildNote(Instrument.One, Notes.E4), BuildNote(Instrument.Two, Notes.G3)),
                BuildChord(BuildNote(Instrument.One, Notes.F4), BuildNote(Instrument.Two, Notes.A3)),
                BuildChord(BuildNote(Instrument.One, Notes.G4), BuildNote(Instrument.Two, Notes.B3)),
                BuildChord(BuildNote(Instrument.One, Notes.A4), BuildNote(Instrument.Two, Notes.C4))
            )
        };

        var motifDeveloper = BuildMotifDeveloper(motifApplicator, configuration, new SeededRandomProvider(Seed));

        // act
        var developed = motifDeveloper.TryDevelop(motifBank, phrase);

        // assert
        developed.Should().NotBeNull();

        var phraseOne = phrase[0].Beats.Select(static beat => beat[Instrument.One].Raw).ToList();
        var phraseTwo = phrase[0].Beats.Select(static beat => beat[Instrument.Two].Raw).ToList();
        var developedOne = developed![0].Beats.Select(static beat => beat[Instrument.One].Raw).ToList();
        var developedTwo = developed[0].Beats.Select(static beat => beat[Instrument.Two].Raw).ToList();

        developedOne.Should().NotEqual(phraseOne, "AllVoices develops Instrument.One");
        developedTwo.Should().NotEqual(phraseTwo, "AllVoices develops Instrument.Two");
    }

    [Test]
    public void TryDevelop_NullMotifBank_ThrowsArgumentNullException()
    {
        // arrange
        var configuration = ConfigurationWith(MotifDevelopmentConfiguration.Default);
        var (_, phrase) = SingleVoiceFixture();
        var motifDeveloper = BuildMotifDeveloper(new MotifApplicator(configuration), configuration, new SeededRandomProvider(Seed));

        // act
        var act = () => motifDeveloper.TryDevelop(null!, phrase);

        // assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("motifBank");
    }

    [Test]
    public void TryDevelop_NullPhrase_ThrowsArgumentNullException()
    {
        // arrange
        var configuration = ConfigurationWith(MotifDevelopmentConfiguration.Default);
        var (motifBank, _) = SingleVoiceFixture();
        var motifDeveloper = BuildMotifDeveloper(new MotifApplicator(configuration), configuration, new SeededRandomProvider(Seed));

        // act
        var act = () => motifDeveloper.TryDevelop(motifBank, null!);

        // assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("phrase");
    }

    private static (MotifBank MotifBank, List<Measure> Phrase) SingleVoiceFixture()
    {
        var motif = new Motif([
            new MotivicGesture(0, MusicalTimeSpan.Half),
            new MotivicGesture(-1, MusicalTimeSpan.Half)
        ]);

        var motifBank = new MotifBank(new Dictionary<Instrument, AnchoredMotif>
        {
            [Instrument.One] = new AnchoredMotif(motif, 0, Instrument.One)
        });

        var phrase = new List<Measure>
        {
            BuildMeasure(
                BuildChord(BuildNote(Instrument.One, Notes.E4)),
                BuildChord(BuildNote(Instrument.One, Notes.F4))
            )
        };

        return (motifBank, phrase);
    }

    private static MotifDeveloper BuildMotifDeveloper(IMotifApplicator motifApplicator, CompositionConfiguration configuration, IRandomProvider randomProvider) =>
        new(motifApplicator, new WeightedRandomBooleanGenerator(randomProvider), randomProvider, configuration);

    private static CompositionConfiguration ConfigurationWith(MotifDevelopmentConfiguration motifDevelopmentConfiguration) =>
        TestCompositionConfigurations.Get(2) with { MotifDevelopmentConfiguration = motifDevelopmentConfiguration };

    private static Measure BuildMeasure(params BaroquenChord[] chords) =>
        new(chords.Select(static chord => new Beat(chord)).ToList(), Meter.FourFour);

    private static BaroquenChord BuildChord(params BaroquenNote[] notes) => new([.. notes]);

    private static BaroquenNote BuildNote(Instrument instrument, Note raw) => new(instrument, raw, MusicalTimeSpan.Half);
}
