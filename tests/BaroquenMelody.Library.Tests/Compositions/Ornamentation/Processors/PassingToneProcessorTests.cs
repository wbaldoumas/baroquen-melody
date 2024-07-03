using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Infrastructure.Collections;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Processors;

[TestFixture]
internal sealed class PassingToneProcessorTests
{
    private PassingToneProcessor _processor = null!;

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

        _processor = new PassingToneProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, OrnamentationType.PassingTone);
    }

    [Test]
    public void Process_applies_descending_passing_tone_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.A4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.F4)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.PassingTone);
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G4);
        noteToAssert.Duration.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[0].Duration.Should().Be(MusicalTimeSpan.Eighth);
    }

    [Test]
    public void Process_applies_ascending_passing_tone_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.A4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C5)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.PassingTone);
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B4);
        noteToAssert.Duration.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[0].Duration.Should().Be(MusicalTimeSpan.Eighth);
    }
}
