using BaroquenMelody.Infrastructure.Collections;
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
internal sealed class PedalProcessorTests
{
    private OrnamentationProcessor _rootPedalProcessor = null!;

    private OrnamentationProcessor _thirdPedalProcessor = null!;

    private OrnamentationProcessor _fifthPedalProcessor = null!;

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

        var configurations = ornamentationProcessorConfigurationFactory.Create(
            new OrnamentationConfiguration(
                OrnamentationType.Pedal,
                ConfigurationStatus.Enabled,
                Probability: 100
            )
        ).ToList();

        _rootPedalProcessor = new OrnamentationProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, configurations[0]);
        _thirdPedalProcessor = new OrnamentationProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, configurations[1]);
        _fifthPedalProcessor = new OrnamentationProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, configurations[2]);
    }

    [Test]
    public void Process_applies_ascending_root_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)]))
        );

        // act
        _rootPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G3);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_descending_root_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A3, MusicalTimeSpan.Half)]))
        );

        // act
        _rootPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G3);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_ascending_third_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half)]))
        );

        // act
        _thirdPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_descending_third_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))
        );

        // act
        _thirdPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.D4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_ascending_fifth_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Half)]))
        );

        // act
        _fifthPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.A4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_applies_descending_fifth_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half)]))
        );

        // act
        _fifthPedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Pedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.C4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.C4);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }
}
