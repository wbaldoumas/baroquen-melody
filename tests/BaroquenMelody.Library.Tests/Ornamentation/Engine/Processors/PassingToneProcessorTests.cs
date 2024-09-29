using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class PassingToneProcessorTests
{
    private PassingToneProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _processor = new PassingToneProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, OrnamentationType.PassingTone);
    }

    [Test]
    public void Process_applies_descending_passing_tone_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.PassingTone);
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G4);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
    }

    [Test]
    public void Process_applies_ascending_passing_tone_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.PassingTone);
        noteToAssert.Ornamentations.Should().ContainSingle();
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B4);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
    }
}
