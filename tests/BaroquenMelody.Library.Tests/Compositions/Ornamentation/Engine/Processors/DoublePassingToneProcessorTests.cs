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
internal sealed class DoublePassingToneProcessorTests
{
    private DoublePassingToneProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(2);

        _processor = new DoublePassingToneProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, OrnamentationType.DoublePassingTone);
    }

    [Test]
    public void Process_applies_descending_passing_tone_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.A4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.E4, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DoublePassingTone);
        noteToAssert.Ornamentations.Should().HaveCount(2);
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[1].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
    }

    [Test]
    public void Process_applies_ascending_passing_tone_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.A4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.D5, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DoublePassingTone);
        noteToAssert.Ornamentations.Should().HaveCount(2);
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.C5);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[1].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
    }
}
