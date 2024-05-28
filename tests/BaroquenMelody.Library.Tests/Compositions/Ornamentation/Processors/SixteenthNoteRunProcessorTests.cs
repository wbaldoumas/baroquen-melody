using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using BaroquenMelody.Library.Infrastructure.Collections;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Processors;

[TestFixture]
internal sealed class SixteenthNoteRunProcessorTests
{
    private SixteenthNoteRunProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C3, Notes.C5),
                new(Voice.Alto, Notes.C2, Notes.C4)
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _processor = new SixteenthNoteRunProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration);
    }

    [Test]
    public void Process_applies_descending_sixteenth_note_run_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.F3)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.A3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G3);

        noteToAssert.Ornamentations[0].Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        noteToAssert.Ornamentations[1].Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        noteToAssert.Ornamentations[2].Duration.Should().Be(MusicalTimeSpan.Sixteenth);

        noteToAssert.Duration.Should().Be(MusicalTimeSpan.Sixteenth);
    }

    [Test]
    public void Process_applies_ascending_sixteenth_note_run_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.F3)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.A3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.B3);

        noteToAssert.Ornamentations[0].Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        noteToAssert.Ornamentations[1].Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        noteToAssert.Ornamentations[2].Duration.Should().Be(MusicalTimeSpan.Sixteenth);

        noteToAssert.Duration.Should().Be(MusicalTimeSpan.Sixteenth);
    }
}
