﻿using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
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
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class MordentProcessorTests
{
    private IWeightedRandomBooleanGenerator _mockWeightedRandomBooleanGenerator;

    private MordentProcessor _mordentProcessor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        _mockWeightedRandomBooleanGenerator = Substitute.For<IWeightedRandomBooleanGenerator>();

        _mordentProcessor = new MordentProcessor(new MusicalTimeSpanCalculator(), _mockWeightedRandomBooleanGenerator, compositionConfiguration);
    }

    [Test]
    public void Process_applies_upper_mordent_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half)]))
        );

        _mockWeightedRandomBooleanGenerator.IsTrue().Returns(true);

        // act
        _mordentProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Mordent);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);

        ornamentationItem.CurrentBeat[0].Ornamentations.Should().HaveCount(2);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B4);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.A4);
        noteToAssert.Ornamentations[1].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter.Dotted(1));
    }

    [Test]
    public void Process_applies_lower_mordent_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half)]))
        );

        _mockWeightedRandomBooleanGenerator.IsTrue().Returns(false);

        // act
        _mordentProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Mordent);
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);

        ornamentationItem.CurrentBeat[0].Ornamentations.Should().HaveCount(2);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G4);
        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Sixteenth);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.A4);
        noteToAssert.Ornamentations[1].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Quarter.Dotted(1));
    }
}
