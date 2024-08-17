using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Infrastructure.Collections;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class AlternateTurnProcessorTests
{
    private AlternateTurnProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        _processor = new AlternateTurnProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration);
    }

    [Test]
    public void Process_applies_ascending_turn_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.AlternateTurn);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);

        foreach (var baroquenNote in noteToAssert.Ornamentations)
        {
            baroquenNote.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_descending_turn_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.B3, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.AlternateTurn);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);

        foreach (var baroquenNote in noteToAssert.Ornamentations)
        {
            baroquenNote.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }
}
