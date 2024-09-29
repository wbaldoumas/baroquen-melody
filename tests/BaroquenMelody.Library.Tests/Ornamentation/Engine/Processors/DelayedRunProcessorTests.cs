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
internal sealed class DelayedRunProcessorTests
{
    private DelayedRunProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _processor = new DelayedRunProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration);
    }

    [Test]
    public void Process_applies_ascending_double_run_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DelayedRun);
        noteToAssert.Ornamentations.Should().HaveCount(4);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.E4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[3].Raw.Should().Be(Notes.G4);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);

        foreach (var baroquenNote in noteToAssert.Ornamentations)
        {
            baroquenNote.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }

    [Test]
    public void Process_applies_descending_double_run_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DelayedRun);
        noteToAssert.Ornamentations.Should().HaveCount(4);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.E4);
        noteToAssert.Ornamentations[3].Raw.Should().Be(Notes.D4);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);

        foreach (var baroquenNote in noteToAssert.Ornamentations)
        {
            baroquenNote.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }
}
