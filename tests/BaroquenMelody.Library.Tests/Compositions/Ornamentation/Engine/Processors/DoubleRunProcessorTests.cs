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
internal sealed class DoubleRunProcessorTests
{
    private DoubleRunProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        _processor = new DoubleRunProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration);
    }

    [Test]
    public void Process_applies_descending_sixteenth_note_run_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.E3, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DoubleRun);
        noteToAssert.Ornamentations.Should().HaveCount(7);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.A3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[3].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[4].Raw.Should().Be(Notes.A3);
        noteToAssert.Ornamentations[5].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[6].Raw.Should().Be(Notes.F3);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);

        foreach (var baroquenNote in noteToAssert.Ornamentations)
        {
            baroquenNote.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }

    [Test]
    public void Process_applies_ascending_sixteenth_note_run_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.One, Notes.A4, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DoubleRun);
        noteToAssert.Ornamentations.Should().HaveCount(7);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.E4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[3].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[4].Raw.Should().Be(Notes.E4);
        noteToAssert.Ornamentations[5].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[6].Raw.Should().Be(Notes.G4);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);

        foreach (var baroquenNote in noteToAssert.Ornamentations)
        {
            baroquenNote.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }
}
