using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Engine.Processors.Factories;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class OctavePedalProcessorTests
{
    private OrnamentationProcessor _octavePedalProcessor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2, tonic: NoteName.B, mode: Mode.Aeolian);

        var ornamentationProcessorConfigurationFactory = new OrnamentationProcessorConfigurationFactory(
            new ChordNumberIdentifier(compositionConfiguration),
            new WeightedRandomBooleanGenerator(),
            compositionConfiguration
        );

        var configurations = ornamentationProcessorConfigurationFactory.Create(
            new OrnamentationConfiguration(
                OrnamentationType.OctavePedal,
                ConfigurationStatus.Enabled,
                Probability: 100
            )
        ).ToList();

        _octavePedalProcessor = new OrnamentationProcessor(new MusicalTimeSpanCalculator(), compositionConfiguration, configurations[0]);
    }

    [Test]
    public void Process_applies_octave_pedal_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.B2, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.D3, MusicalTimeSpan.Half)]))
        );

        // act
        _octavePedalProcessor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.OctavePedal);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B1);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.B2);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.B1);

        foreach (var ornamentation in noteToAssert.Ornamentations)
        {
            ornamentation.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        }
    }

    // In B Aeolian the scale's note list starts at B(-1)/C0, so A0 sits at index 6 and the -7 translations land at
    // index -1: the site cannot carry the figure, and the processor must leave the note untouched instead of throwing.
    [Test]
    public void Process_leaves_the_note_untouched_when_a_translation_falls_below_the_scale_list()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.A0, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.B0, MusicalTimeSpan.Half)]))
        );

        // act
        var act = () => _octavePedalProcessor.Process(ornamentationItem);

        // assert
        act.Should().NotThrow("a figure whose translations leave the scale's note list simply does not fit the site");

        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.None);
        noteToAssert.Ornamentations.Should().BeEmpty();
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Half);
    }
}
