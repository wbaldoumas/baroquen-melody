﻿using BaroquenMelody.Library.Compositions.Configurations;
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
internal sealed class DoublePassingToneProcessorTests
{
    private DoublePassingToneProcessor _processor = null!;

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

        _processor = new DoublePassingToneProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, OrnamentationType.DoublePassingTone);
    }

    [Test]
    public void Process_applies_descending_passing_tone_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.A4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.E4)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DoublePassingTone);
        noteToAssert.Ornamentations.Should().HaveCount(2);
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.Duration.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[0].Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        noteToAssert.Ornamentations[1].Duration.Should().Be(MusicalTimeSpan.Sixteenth);
    }

    [Test]
    public void Process_applies_ascending_passing_tone_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Voice.Soprano,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.A4)])),
            new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.D5)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Voice.Soprano];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DoublePassingTone);
        noteToAssert.Ornamentations.Should().HaveCount(2);
        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.C5);
        noteToAssert.Duration.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[0].Duration.Should().Be(MusicalTimeSpan.Sixteenth);
        noteToAssert.Ornamentations[1].Duration.Should().Be(MusicalTimeSpan.Sixteenth);
    }
}