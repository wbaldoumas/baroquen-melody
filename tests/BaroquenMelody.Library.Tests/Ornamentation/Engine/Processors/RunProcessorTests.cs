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
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class RunProcessorTests
{
    private OrnamentationProcessor _processor = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2);

        var ornamentationProcessorConfigurationFactory = new OrnamentationProcessorConfigurationFactory(
            new ChordNumberIdentifier(compositionConfiguration),
            new WeightedRandomBooleanGenerator(),
            compositionConfiguration
        );

        var configuration = ornamentationProcessorConfigurationFactory.Create(
            new OrnamentationConfiguration(
                OrnamentationType.Run,
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
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.F3, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Run);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.B3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.A3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.G3);

        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[1].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[2].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
    }

    [Test]
    public void Process_applies_ascending_sixteenth_note_run_as_expected()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.F3, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.Run);
        noteToAssert.Ornamentations.Should().HaveCount(3);

        noteToAssert.Ornamentations[0].Raw.Should().Be(Notes.G3);
        noteToAssert.Ornamentations[1].Raw.Should().Be(Notes.A3);
        noteToAssert.Ornamentations[2].Raw.Should().Be(Notes.B3);

        noteToAssert.Ornamentations[0].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[1].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
        noteToAssert.Ornamentations[2].MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);

        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Eighth);
    }

    // A note outside the scale cannot pivot the run's scale-relative translations: F# is not in C Ionian, and
    // falling through with IndexOf's -1 pivot would build the run from the very bottom of the note list (C-1, D-1,
    // E-1). Unreachable during composition (every composed note is drawn from the scale), but the processor must
    // leave such a site untouched for any input.
    [Test]
    public void Process_leaves_the_note_untouched_when_the_current_note_is_not_in_the_scale()
    {
        // arrange
        var ornamentationItem = new OrnamentationItem(
            Instrument.One,
            new FixedSizeList<Beat>(1),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.FSharp4, MusicalTimeSpan.Half)])),
            new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C5, MusicalTimeSpan.Half)]))
        );

        // act
        _processor.Process(ornamentationItem);

        // assert
        var noteToAssert = ornamentationItem.CurrentBeat[Instrument.One];

        noteToAssert.OrnamentationType.Should().Be(OrnamentationType.None);
        noteToAssert.Ornamentations.Should().BeEmpty();
        noteToAssert.MusicalTimeSpan.Should().Be(MusicalTimeSpan.Half);
    }
}
