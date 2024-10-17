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
internal sealed class DecorateIntervalProcessorTests
{
    private OrnamentationProcessor _decorateIntervalBelowSupertonic;

    private OrnamentationProcessor _decorateIntervalAboveSupertonic;

    private OrnamentationProcessor _decorateIntervalBelowLeadingTone;

    private OrnamentationProcessor _decorateIntervalAboveLeadingTone;

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
                OrnamentationType.DecorateInterval,
                ConfigurationStatus.Enabled,
                Probability: 100
            )
        ).ToList();

        // Note: This test has secret knowledge of ordering of the configurations returned by OrnamentationProcessorConfigurationFactory.
        _decorateIntervalBelowSupertonic = new OrnamentationProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, configurations[0]);
        _decorateIntervalAboveSupertonic = new OrnamentationProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, configurations[1]);
        _decorateIntervalAboveLeadingTone = new OrnamentationProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, configurations[2]);
        _decorateIntervalBelowLeadingTone = new OrnamentationProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, configurations[3]);
    }

    [Test]
    public void Process_decorates_interval_as_expected_below_supertonic()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)])),
            null
        );

        // act
        _decorateIntervalBelowSupertonic.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DecorateInterval);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G3);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);

        foreach (var note in noteToAssert.Ornamentations)
        {
            note.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_decorates_interval_as_expected_above_supertonic()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half)])),
            null
        );

        // act
        _decorateIntervalAboveSupertonic.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DecorateInterval);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G4);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);

        foreach (var note in noteToAssert.Ornamentations)
        {
            note.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_decorates_interval_as_expected_below_leading_tone()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Half)])),
            null
        );

        // act
        _decorateIntervalBelowLeadingTone.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DecorateInterval);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G4);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F4);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G4);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);

        foreach (var note in noteToAssert.Ornamentations)
        {
            note.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    public void Process_decorates_interval_as_expected_above_leading_tone()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Half)])),
            null
        );

        // act
        _decorateIntervalAboveLeadingTone.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.DecorateInterval);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G5);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.F5);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G5);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);

        foreach (var note in noteToAssert.Ornamentations)
        {
            note.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }
}
