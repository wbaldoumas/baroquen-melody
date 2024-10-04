using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Dynamics;
using BaroquenMelody.Library.Dynamics.Engine.Processors;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Dynamics.Engine.Processors;

[TestFixture]
internal sealed class InitialDynamicsProcessorTests
{
    [Test]
    public void When_instrument_configuration_has_sizeable_velocity_range_target_velocity_ratio_is_used()
    {
        // arrange
        var instrumentConfiguration = new InstrumentConfiguration(
            Instrument.One,
            Notes.C4,
            Notes.C6,
            MinVelocity: new SevenBitNumber(50),
            MaxVelocity: new SevenBitNumber(75),
            GeneralMidi2Program.AcousticGrandPiano,
            ConfigurationStatus.Enabled
        );

        var configuration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration> { instrumentConfiguration },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100
        );

        var initialDynamicsProcessor = new InitialDynamicsProcessor(configuration, velocityRangeTarget: 0.5);

        // expected velocity is instrumentConfiguration.MinVelocity + instrumentConfiguration.VelocityRange * velocityRangeTarget;
        var expectedVelocity = new SevenBitNumber(62);

        var currentBeatNote = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Sixteenth)
        {
            OrnamentationType = OrnamentationType.Run,
            Ornamentations =
            {
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Sixteenth),
                new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Sixteenth)
            }
        };

        var item = new DynamicsApplicationItem
        {
            Instrument = Instrument.One,
            PrecedingBeats = new FixedSizeList<Beat>(2),
            CurrentBeat = new Beat(new BaroquenChord([currentBeatNote])),
            NextBeat = new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            ProcessedInstruments = []
        };

        // act
        initialDynamicsProcessor.Process(item);

        // assert
        var processedNote = item.CurrentBeat[Instrument.One];

        processedNote.Velocity.Should().Be(expectedVelocity);

        foreach (var ornamentation in processedNote.Ornamentations)
        {
            ornamentation.Velocity.Should().Be(expectedVelocity);
        }
    }

    [Test]
    public void When_instrument_configuration_has_no_sizeable_velocity_range_max_velocity_is_used()
    {
        // arrange
        var instrumentConfiguration = new InstrumentConfiguration(
            Instrument.One,
            Notes.C4,
            Notes.C6,
            MinVelocity: new SevenBitNumber(72),
            MaxVelocity: new SevenBitNumber(75),
            GeneralMidi2Program.AcousticGrandPiano,
            ConfigurationStatus.Enabled
        );

        var configuration = new CompositionConfiguration(
            new HashSet<InstrumentConfiguration> { instrumentConfiguration },
            PhrasingConfiguration.Default,
            AggregateCompositionRuleConfiguration.Default,
            AggregateOrnamentationConfiguration.Default,
            NoteName.C,
            Mode.Ionian,
            Meter.FourFour,
            MusicalTimeSpan.Half,
            MinimumMeasures: 100
        );

        var initialDynamicsProcessor = new InitialDynamicsProcessor(configuration, velocityRangeTarget: 0.5);

        // expected velocity is instrumentConfiguration.MaxVelocity;
        var expectedVelocity = new SevenBitNumber(75);

        var item = new DynamicsApplicationItem
        {
            Instrument = Instrument.One,
            PrecedingBeats = new FixedSizeList<Beat>(2),
            CurrentBeat = new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            NextBeat = new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half)])),
            ProcessedInstruments = []
        };

        // act
        initialDynamicsProcessor.Process(item);

        // assert
        item.CurrentBeat[Instrument.One].Velocity.Should().Be(expectedVelocity);
    }
}
