using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
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
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

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
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)]))
        );

        // act
        _rootPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G3);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_descending_root_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A3, MusicalTimeSpan.Half)]))
        );

        // act
        _rootPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G3);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_ascending_third_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half)]))
        );

        // act
        _thirdPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_descending_third_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))
        );

        // act
        _thirdPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_ascending_fifth_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Half)]))
        );

        // act
        _fifthPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.A4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_descending_fifth_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)]))
        );

        // act
        _fifthPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }
}
