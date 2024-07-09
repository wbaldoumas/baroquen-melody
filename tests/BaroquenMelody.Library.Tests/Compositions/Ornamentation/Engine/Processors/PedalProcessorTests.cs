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

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class PedalProcessorTests
{
    private PedalProcessor _rootPedalProcessor = null!;

    private PedalProcessor _thirdPedalProcessor = null!;

    private PedalProcessor _fifthPedalProcessor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C3, Notes.G6),
                new(Voice.Alto, Notes.C2, Notes.G5),
                new(Voice.Tenor, Notes.C1, Notes.G4)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        var musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();

        _rootPedalProcessor = new PedalProcessor(
            musicalTimeSpanCalculator,
            compositionConfiguration,
            PedalProcessor.RootPedalInterval
        );

        _thirdPedalProcessor = new PedalProcessor(
            musicalTimeSpanCalculator,
            compositionConfiguration,
            PedalProcessor.ThirdPedalInterval
        );

        _fifthPedalProcessor = new PedalProcessor(
            musicalTimeSpanCalculator,
            compositionConfiguration,
            PedalProcessor.FifthPedalInterval
        );
    }

    [Test]
    public void Process_applies_ascending_root_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.E4)]))
        );

        // act
        _rootPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G3);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }

    [Test]
    public void Process_applies_descending_root_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.A3)]))
        );

        // act
        _rootPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G3);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }

    [Test]
    public void Process_applies_ascending_third_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.E4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.G4)]))
        );

        // act
        _thirdPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }

    [Test]
    public void Process_applies_descending_third_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.E4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4)]))
        );

        // act
        _thirdPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }

    [Test]
    public void Process_applies_ascending_fifth_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.G4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.B4)]))
        );

        // act
        _fifthPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.A4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }

    [Test]
    public void Process_applies_descending_fifth_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.G4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.E4)]))
        );

        // act
        _fifthPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }
}
