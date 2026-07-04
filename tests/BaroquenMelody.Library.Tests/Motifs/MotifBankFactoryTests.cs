using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Motifs;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Motifs;

[TestFixture]
internal sealed class MotifBankFactoryTests
{
    [Test]
    public void Create_CatalogsOneAnchoredMotifPerThemeVoice_WithTheEntryHeadAsAnchor()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(2);
        var factory = new MotifBankFactory(new MotifExtractor(configuration), configuration);
        var scale = configuration.Scale;

        // act
        var bank = factory.Create(BuildStaggeredTheme());

        // assert: each voice's anchor is the scale index of its first exposition entry note.
        bank.Voices.Should().BeEquivalentTo([Instrument.One, Instrument.Two]);
        bank.Get(Instrument.One).Instrument.Should().Be(Instrument.One);
        bank.Get(Instrument.Two).Instrument.Should().Be(Instrument.Two);
        bank.Get(Instrument.One).AnchorScaleIndex.Should().Be(scale.IndexOf(BuildNote(Instrument.One, Notes.C4)));
        bank.Get(Instrument.Two).AnchorScaleIndex.Should().Be(scale.IndexOf(BuildNote(Instrument.Two, Notes.G3)));
    }

    [Test]
    public void Create_MotifMatchesAnIndependentExtractionForEachVoice()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(2);
        var extractor = new MotifExtractor(configuration);
        var factory = new MotifBankFactory(extractor, configuration);

        // act
        var bank = factory.Create(BuildStaggeredTheme());

        // assert: each voice's motif is its first-appearance exposition line read per beat.
        var expectedOne = extractor.Extract([BuildNote(Instrument.One, Notes.C4), BuildNote(Instrument.One, Notes.E4)]);
        var expectedTwo = extractor.Extract([BuildNote(Instrument.Two, Notes.G3), BuildNote(Instrument.Two, Notes.A3)]);

        bank.Get(Instrument.One).Motif.Gestures.Should().Equal(expectedOne.Gestures);
        bank.Get(Instrument.Two).Motif.Gestures.Should().Equal(expectedTwo.Gestures);
    }

    [Test]
    public void CreateThenApplyIdentity_RoundTripsToEachVoiceEntryLine()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(2);
        var factory = new MotifBankFactory(new MotifExtractor(configuration), configuration);
        var applicator = new MotifApplicator(configuration);

        // act
        var bank = factory.Create(BuildStaggeredTheme());

        // assert: bank + applicator reproduce each voice's exposition entry line verbatim.
        applicator.Apply(bank.Get(Instrument.One)).Should().Equal(
            BuildNote(Instrument.One, Notes.C4),
            BuildNote(Instrument.One, Notes.E4)
        );

        applicator.Apply(bank.Get(Instrument.Two)).Should().Equal(
            BuildNote(Instrument.Two, Notes.G3),
            BuildNote(Instrument.Two, Notes.A3)
        );
    }

    [Test]
    public void Create_OmitsVoicesAbsentFromTheTheme_AndYieldsAnEmptyBankForAnEmptyTheme()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(2);
        var factory = new MotifBankFactory(new MotifExtractor(configuration), configuration);

        var singleVoiceTheme = new BaroquenTheme(
            [BuildMeasure(BuildChord(BuildNote(Instrument.One, Notes.C4)))],
            [BuildMeasure(BuildChord(BuildNote(Instrument.One, Notes.C4)))]
        );

        var emptyTheme = new BaroquenTheme([], []);

        // act
        var singleVoiceBank = factory.Create(singleVoiceTheme);
        var emptyBank = factory.Create(emptyTheme);

        // assert
        singleVoiceBank.Voices.Should().BeEquivalentTo([Instrument.One]);
        singleVoiceBank.Contains(Instrument.Two).Should().BeFalse();
        emptyBank.Voices.Should().BeEmpty();
    }

    [Test]
    public void Create_NullTheme_ThrowsArgumentNullException()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(2);
        var factory = new MotifBankFactory(new MotifExtractor(configuration), configuration);

        // act
        var act = () => factory.Create(null!);

        // assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("theme");
    }

    [Test]
    public void BankLookup_UnknownInstrument_TryGetReturnsFalse_AndGetThrows()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(2);
        var factory = new MotifBankFactory(new MotifExtractor(configuration), configuration);
        var bank = factory.Create(BuildStaggeredTheme());

        // act
        var getUnknown = () => bank.Get(Instrument.Three);

        // assert: Instrument.Three never appears in the theme.
        bank.Contains(Instrument.Three).Should().BeFalse();
        bank.TryGet(Instrument.Three, out var anchoredMotif).Should().BeFalse();
        anchoredMotif.Should().BeNull();
        getUnknown.Should().Throw<KeyNotFoundException>();
    }

    // A staggered two-voice exposition: Instrument.One enters in measure 0, Instrument.Two in measure 1, so each
    // voice's "entry line" is the first measure it appears in (One -> [C4, E4]; Two -> [G3, A3]).
    private static BaroquenTheme BuildStaggeredTheme()
    {
        var exposition = new List<Measure>
        {
            BuildMeasure(
                BuildChord(BuildNote(Instrument.One, Notes.C4)),
                BuildChord(BuildNote(Instrument.One, Notes.E4))
            ),
            BuildMeasure(
                BuildChord(BuildNote(Instrument.One, Notes.G4), BuildNote(Instrument.Two, Notes.G3)),
                BuildChord(BuildNote(Instrument.One, Notes.F4), BuildNote(Instrument.Two, Notes.A3))
            )
        };

        return new BaroquenTheme(exposition, exposition);
    }

    private static Measure BuildMeasure(params BaroquenChord[] chords) =>
        new(chords.Select(chord => new Beat(chord)).ToList(), Meter.FourFour);

    private static BaroquenChord BuildChord(params BaroquenNote[] notes) => new(notes.ToList());

    private static BaroquenNote BuildNote(Instrument instrument, Note raw) => new(instrument, raw, MusicalTimeSpan.Half);
}
