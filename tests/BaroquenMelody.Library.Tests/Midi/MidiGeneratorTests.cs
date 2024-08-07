using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Midi;
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
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C3, Notes.C5),
                new(Voice.Alto, Notes.C2, Notes.C4)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _midiGenerator = new MidiGenerator(compositionConfiguration);
    }

    [Test]
    public void Generate_returns_midi_file_as_expected()
    {
        // arrange
        var sopranoWithMidSustain = new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.MidSustain
        };

        var altoWithRest = new BaroquenNote(Voice.Alto, Notes.C3, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.Rest
        };

        var sopranoWithPassingTone = new BaroquenNote(Voice.Soprano, Notes.D4, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.PassingTone,
            Ornamentations = { new BaroquenNote(Voice.Soprano, Notes.C4, MusicalTimeSpan.Half) }
        };

        var altoWithDoubleTurn = new BaroquenNote(Voice.Alto, Notes.C3, MusicalTimeSpan.Half)
        {
            OrnamentationType = OrnamentationType.DoubleTurn,
            Ornamentations = { new BaroquenNote(Voice.Alto, Notes.B3, MusicalTimeSpan.Half), new BaroquenNote(Voice.Alto, Notes.D4, MusicalTimeSpan.Half) }
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
        midiFile.Chunks.Should().HaveCount(2, "because there are two voices");
    }
}
