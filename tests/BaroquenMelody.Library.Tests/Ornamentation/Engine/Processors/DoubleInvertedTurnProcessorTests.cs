﻿using BaroquenMelody.Infrastructure.Collections;
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
internal sealed class DoubleInvertedTurnProcessorTests
{
    private DoubleInvertedTurnProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _processor = new DoubleInvertedTurnProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration);
    }

    [Test]
    public void Process_applies_descending_sixteenth_note_run_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A3, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DoubleInvertedTurn);
        noteToAssert.Ornamentations.Should().HaveCount(7);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[3].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[4].Raw.Should().Be(Notes.A3);
        noteToAssert.Ornamentations[5].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[6].Raw.Should().Be(Notes.B3);

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
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A3, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DoubleInvertedTurn);
        noteToAssert.Ornamentations.Should().HaveCount(7);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.A3);
        noteToAssert.Ornamentations[3].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[4].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[5].Raw.Should().Be(Notes.A3);
        noteToAssert.Ornamentations[6].Raw.Should().Be(Notes.B3);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);

        foreach (var baroquenNote in noteToAssert.Ornamentations)
        {
            baroquenNote.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);
        }
    }
}
