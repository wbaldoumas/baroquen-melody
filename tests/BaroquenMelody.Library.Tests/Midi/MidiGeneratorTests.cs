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
        var compositionConfiguration = TestCompositionConfigurations.GetCompositionConfiguration(2);

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

        var altoWithDoubleTurn = new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.DoubleTurn,
            Ornamentations = { new BaroquenNote(Instrument.Two, Notes.B3, MusicalTimeSpan.Half), new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Half) }
        };

        var composition = new Composition(
            [
                new Measure(
                    [
                        new Beat(new BaroquenChord([sopranoWithMidSustain])),
                        new Beat(new BaroquenChord([sopranoWithPassingTone, altoWithDoubleTurn])),
                        new Beat(new BaroquenChord([sopranoWithMidSustain, altoWithRest])),
                        new Beat(new BaroquenChord([sopranoWithPassingTone, altoWithDoubleTurn]))
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
}
