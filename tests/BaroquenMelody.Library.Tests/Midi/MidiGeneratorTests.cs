using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Midi;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Midi;

[TestFixture]
internal sealed class MidiGeneratorTests
{
    private MidiGenerator _midiGenerator = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _midiGenerator = new MidiGenerator(compositionConfiguration);
    }

    [Test]
    public void Generate_returns_midi_file_as_expected()
    {
        // arrange
        var sopranoWithMidSustain = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.MidSustain
        };

        var altoWithRest = new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.Rest
        };

        var sopranoWithPassingTone = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.PassingTone,
            Ornamentations = { new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) }
        };

        var altoWithDoubleInvertedTurn = new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.DoubleInvertedTurn,
            Ornamentations = { new BaroquenNote(Instrument.Two, Notes.B3, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Half) }
        };

        var composition = new Composition(
            [
                new Measure(
                    [
                        new Beat(new BaroquenChord([sopranoWithMidSustain])),
                        new Beat(new BaroquenChord([sopranoWithPassingTone, altoWithDoubleInvertedTurn])),
                        new Beat(new BaroquenChord([sopranoWithMidSustain, altoWithRest])),
                        new Beat(new BaroquenChord([sopranoWithPassingTone, altoWithDoubleInvertedTurn]))
                    ],
                    Meter.FourFour
                )
            ]
        );

        // act
        var midiFile = _midiGenerator.Generate(composition);

        // assert
        midiFile.Should().NotBeNull();
        midiFile.Chunks.Should().HaveCount(2, "because there are two instruments");
    }

    // Audit: ui-razor-components-1 - demonstration (passes by design): a tempo of 2 BPM is not encodable in a MIDI
    // set-tempo event (30,000,000 us per quarter exceeds the 24-bit ceiling), so the generator throws after the whole
    // composition has run. The UI admits tempos 1..3 through its Min="1" bound; the clamp belongs in the card.
    [Test]
    public void Generate_ThrowsForATempoBelowTheMidiEncodableMinimum()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get(2) with { Tempo = 2 };
        var midiGenerator = new MidiGenerator(compositionConfiguration);

        var composition = new Composition(
            [
                new Measure([new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))], Meter.FourFour)
            ]
        );

        // act
        var act = () => midiGenerator.Generate(composition);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Generate_does_not_sound_the_silent_principal_of_an_appoggiatura()
    {
        // arrange
        var appoggiatura = new BaroquenNote(Instrument.One, Notes.C4, new MusicalTimeSpan())
        {
            OrnamentationType = OrnamentationType.Appoggiatura,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Quarter),
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter)
            }
        };

        var composition = new Composition(
            [
                new Measure([new Beat(new BaroquenChord([appoggiatura]))], Meter.FourFour)
            ]
        );

        // act
        var midiFile = _midiGenerator.Generate(composition);

        // assert
        var emittedNotes = midiFile.GetNotes().OrderBy(static note => note.Time).ToList();

        // the silent principal (the chord-tone anchor) is not sounded; only the accented dissonance (D4) and then its resolution (C4) are emitted.
        emittedNotes.Should().HaveCount(2);
        emittedNotes[0].NoteNumber.Should().Be(Notes.D4.NoteNumber);
        emittedNotes[1].NoteNumber.Should().Be(Notes.C4.NoteNumber);
        emittedNotes[1].Time.Should().BeGreaterThan(emittedNotes[0].Time);
    }
}
