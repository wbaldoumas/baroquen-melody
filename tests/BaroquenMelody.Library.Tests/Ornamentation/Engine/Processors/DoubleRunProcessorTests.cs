﻿using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class DoubleRunProcessorTests
{
    private OrnamentationProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        var ornamentationProcessorConfigurationFactory = new OrnamentationProcessorConfigurationFactory(
            new ChordNumberIdentifier(compositionConfiguration),
            new WeightedRandomBooleanGenerator(),
            compositionConfiguration,
            Substitute.For<ILogger>()
        );

        var configuration = ornamentationProcessorConfigurationFactory.Create(
            new OrnamentationConfiguration(
                OrnamentationType.DoubleRun,
                ConfigurationStatus.Enabled,
                Probability: 100
            )
        ).First();

        _processor = new OrnamentationProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, configuration);
    }

    [Test]
    public void Process_applies_descending_sixteenth_note_run_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E3, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

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
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

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
