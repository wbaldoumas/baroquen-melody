using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Reducers;

[TestFixture]
internal sealed class InstrumentConfigurationReducersTests
{
    [Test]
    public void ReduceUpdateInstrumentConfiguration_updates_instrument_configurations_as_expected()
    {
        // arrange
        var state = new InstrumentConfigurationState();

        // act
        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(
            state,
            new UpdateInstrumentConfiguration(
                Instrument.One,
                Notes.C4,
                Notes.C5,
                InstrumentConfiguration.DefaultMinVelocity,
                InstrumentConfiguration.DefaultMaxVelocity,
                GeneralMidi2Program.Accordion,
                Status: ConfigurationStatus.Enabled,
                IsUserApplied: true
            )
        );

        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(
            state,
            new UpdateInstrumentConfiguration(
                Instrument.Two,
                Notes.C5,
                Notes.C6,
                InstrumentConfiguration.DefaultMinVelocity,
                InstrumentConfiguration.DefaultMaxVelocity,
                GeneralMidi2Program.Banjo,
                Status: ConfigurationStatus.Enabled,
                IsUserApplied: true
            )
        );

        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(
            state,
            new UpdateInstrumentConfiguration(
                Instrument.Three,
                Notes.C6,
                Notes.C7,
                InstrumentConfiguration.DefaultMinVelocity,
                InstrumentConfiguration.DefaultMaxVelocity,
                GeneralMidi2Program.Celesta,
                Status: ConfigurationStatus.Enabled,
                IsUserApplied: true
            )
        );

        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(
            state,
            new UpdateInstrumentConfiguration(
                Instrument.One,
                Notes.C7,
                Notes.C8,
                new SevenBitNumber(69),
                new SevenBitNumber(70),
                GeneralMidi2Program.Dulcimer,
                Status: ConfigurationStatus.Disabled,
                IsUserApplied: false
            )
        );

        // assert
        var expectedMinVelocity = new SevenBitNumber(69);
        var expectedMaxVelocity = new SevenBitNumber(70);

        state.Configurations.Should().ContainKeys(Instrument.One, Instrument.Two, Instrument.Three);

        var instrumentOneState = state[Instrument.One]!;
        var instrumentTwoState = state[Instrument.Two]!;
        var instrumentThreeState = state[Instrument.Three]!;

        instrumentOneState.MinNote.Should().Be(Notes.C7);
        instrumentOneState.MaxNote.Should().Be(Notes.C8);
        instrumentOneState.MinVelocity.Should().Be(expectedMinVelocity);
        instrumentOneState.MaxVelocity.Should().Be(expectedMaxVelocity);
        instrumentOneState.MidiProgram.Should().Be(GeneralMidi2Program.Dulcimer);
        instrumentOneState.IsEnabled.Should().BeFalse();

        instrumentTwoState.MinNote.Should().Be(Notes.C5);
        instrumentTwoState.MaxNote.Should().Be(Notes.C6);
        instrumentTwoState.MinVelocity.Should().Be(InstrumentConfiguration.DefaultMinVelocity);
        instrumentTwoState.MaxVelocity.Should().Be(InstrumentConfiguration.DefaultMaxVelocity);
        instrumentTwoState.MidiProgram.Should().Be(GeneralMidi2Program.Banjo);
        instrumentTwoState.IsEnabled.Should().BeTrue();

        instrumentThreeState.MinNote.Should().Be(Notes.C6);
        instrumentThreeState.MaxNote.Should().Be(Notes.C7);
        instrumentThreeState.MinVelocity.Should().Be(InstrumentConfiguration.DefaultMinVelocity);
        instrumentThreeState.MaxVelocity.Should().Be(InstrumentConfiguration.DefaultMaxVelocity);
        instrumentThreeState.MidiProgram.Should().Be(GeneralMidi2Program.Celesta);
        instrumentThreeState.IsEnabled.Should().BeTrue();

        var lastUserAppliedInstrumentOneState = state.LastUserAppliedConfigurations[Instrument.One];
        var lastUserAppliedInstrumentTwoState = state.LastUserAppliedConfigurations[Instrument.Two];
        var lastUserAppliedInstrumentThreeState = state.LastUserAppliedConfigurations[Instrument.Three];

        lastUserAppliedInstrumentOneState.MinNote.Should().Be(Notes.C4);
        lastUserAppliedInstrumentOneState.MaxNote.Should().Be(Notes.C5);
        lastUserAppliedInstrumentTwoState.MinVelocity.Should().Be(InstrumentConfiguration.DefaultMinVelocity);
        lastUserAppliedInstrumentTwoState.MaxVelocity.Should().Be(InstrumentConfiguration.DefaultMaxVelocity);
        lastUserAppliedInstrumentOneState.MidiProgram.Should().Be(GeneralMidi2Program.Accordion);
        lastUserAppliedInstrumentOneState.IsEnabled.Should().BeTrue();

        lastUserAppliedInstrumentTwoState.MinNote.Should().Be(Notes.C5);
        lastUserAppliedInstrumentTwoState.MaxNote.Should().Be(Notes.C6);
        lastUserAppliedInstrumentTwoState.MinVelocity.Should().Be(InstrumentConfiguration.DefaultMinVelocity);
        lastUserAppliedInstrumentTwoState.MaxVelocity.Should().Be(InstrumentConfiguration.DefaultMaxVelocity);
        lastUserAppliedInstrumentTwoState.MidiProgram.Should().Be(GeneralMidi2Program.Banjo);
        lastUserAppliedInstrumentTwoState.IsEnabled.Should().BeTrue();

        lastUserAppliedInstrumentThreeState.MinNote.Should().Be(Notes.C6);
        lastUserAppliedInstrumentThreeState.MaxNote.Should().Be(Notes.C7);
        lastUserAppliedInstrumentThreeState.MinVelocity.Should().Be(InstrumentConfiguration.DefaultMinVelocity);
        lastUserAppliedInstrumentThreeState.MaxVelocity.Should().Be(InstrumentConfiguration.DefaultMaxVelocity);
        lastUserAppliedInstrumentThreeState.MidiProgram.Should().Be(GeneralMidi2Program.Celesta);
        lastUserAppliedInstrumentThreeState.IsEnabled.Should().BeTrue();

        var otherConfiguration = InstrumentConfiguration.DefaultConfigurations[Instrument.Four];

        state[Instrument.Four]!.Should().BeEquivalentTo(otherConfiguration);
        state.LastUserAppliedConfigurations[Instrument.Four].Should().BeEquivalentTo(otherConfiguration);
    }

    [Test]
    public void ReduceUpdateMidiInstrument_updates_instrument_configurations_as_expected()
    {
        // arrange
        var state = new InstrumentConfigurationState();

        // act
        state = InstrumentConfigurationReducers.ReduceUpdateMidiInstrument(
            state,
            new UpdateMidiInstrument(Instrument.One, GeneralMidi2Program.AcousticGrandPiano)
        );

        state = InstrumentConfigurationReducers.ReduceUpdateMidiInstrument(
            state,
            new UpdateMidiInstrument(Instrument.Two, GeneralMidi2Program.Banjo)
        );

        state = InstrumentConfigurationReducers.ReduceUpdateMidiInstrument(
            state,
            new UpdateMidiInstrument(Instrument.Three, GeneralMidi2Program.Celesta)
        );

        // assert
        state.Configurations.Should().ContainKeys(Instrument.One, Instrument.Two, Instrument.Three);

        var instrumentOneState = state[Instrument.One]!;
        var instrumentTwoState = state[Instrument.Two]!;
        var instrumentThreeState = state[Instrument.Three]!;

        var lastUserAppliedInstrumentOneState = state.LastUserAppliedConfigurations[Instrument.One];
        var lastUserAppliedInstrumentTwoState = state.LastUserAppliedConfigurations[Instrument.Two];
        var lastUserAppliedInstrumentThreeState = state.LastUserAppliedConfigurations[Instrument.Three];

        instrumentOneState.MidiProgram.Should().Be(GeneralMidi2Program.AcousticGrandPiano);
        instrumentTwoState.MidiProgram.Should().Be(GeneralMidi2Program.Banjo);
        instrumentThreeState.MidiProgram.Should().Be(GeneralMidi2Program.Celesta);

        lastUserAppliedInstrumentOneState.MidiProgram.Should().Be(GeneralMidi2Program.AcousticGrandPiano);
        lastUserAppliedInstrumentTwoState.MidiProgram.Should().Be(GeneralMidi2Program.Banjo);
        lastUserAppliedInstrumentThreeState.MidiProgram.Should().Be(GeneralMidi2Program.Celesta);
    }

    [Test]
    public void ReduceUpdateInstrumentVelocities_updates_instrument_configurations_as_expected()
    {
        // arrange
        var state = new InstrumentConfigurationState();

        // act
        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentVelocities(
            state,
            new UpdateInstrumentVelocities(Instrument.One, new SevenBitNumber(50), new SevenBitNumber(75))
        );

        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentVelocities(
            state,
            new UpdateInstrumentVelocities(Instrument.Two, new SevenBitNumber(25), new SevenBitNumber(50))
        );

        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentVelocities(
            state,
            new UpdateInstrumentVelocities(Instrument.Three, new SevenBitNumber(0), new SevenBitNumber(25))
        );

        // assert
        state.Configurations.Should().ContainKeys(Instrument.One, Instrument.Two, Instrument.Three);

        var instrumentOneState = state[Instrument.One]!;
        var instrumentTwoState = state[Instrument.Two]!;
        var instrumentThreeState = state[Instrument.Three]!;

        var lastUserAppliedInstrumentOneState = state.LastUserAppliedConfigurations[Instrument.One];
        var lastUserAppliedInstrumentTwoState = state.LastUserAppliedConfigurations[Instrument.Two];
        var lastUserAppliedInstrumentThreeState = state.LastUserAppliedConfigurations[Instrument.Three];

        instrumentOneState.MinVelocity.Should().Be(new SevenBitNumber(50));
        instrumentOneState.MaxVelocity.Should().Be(new SevenBitNumber(75));
        instrumentTwoState.MinVelocity.Should().Be(new SevenBitNumber(25));
        instrumentTwoState.MaxVelocity.Should().Be(new SevenBitNumber(50));
        instrumentThreeState.MinVelocity.Should().Be(new SevenBitNumber(0));
        instrumentThreeState.MaxVelocity.Should().Be(new SevenBitNumber(25));

        lastUserAppliedInstrumentOneState.MinVelocity.Should().Be(new SevenBitNumber(50));
        lastUserAppliedInstrumentOneState.MaxVelocity.Should().Be(new SevenBitNumber(75));
        lastUserAppliedInstrumentTwoState.MinVelocity.Should().Be(new SevenBitNumber(25));
        lastUserAppliedInstrumentTwoState.MaxVelocity.Should().Be(new SevenBitNumber(50));
        lastUserAppliedInstrumentThreeState.MinVelocity.Should().Be(new SevenBitNumber(0));
        lastUserAppliedInstrumentThreeState.MaxVelocity.Should().Be(new SevenBitNumber(25));
    }

    [Test]
    public void ReduceUpdateInstrumentTonalRange_updates_instrument_configurations_as_expected()
    {
        // arrange
        var state = new InstrumentConfigurationState();

        // act
        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentTonalRange(
            state,
            new UpdateInstrumentTonalRange(Instrument.One, Notes.C4, Notes.C5)
        );

        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentTonalRange(
            state,
            new UpdateInstrumentTonalRange(Instrument.Two, Notes.C5, Notes.C6)
        );

        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentTonalRange(
            state,
            new UpdateInstrumentTonalRange(Instrument.Three, Notes.C6, Notes.C7)
        );

        // assert
        state.Configurations.Should().ContainKeys(Instrument.One, Instrument.Two, Instrument.Three);

        var instrumentOneState = state[Instrument.One]!;
        var instrumentTwoState = state[Instrument.Two]!;
        var instrumentThreeState = state[Instrument.Three]!;

        var lastUserAppliedInstrumentOneState = state.LastUserAppliedConfigurations[Instrument.One];
        var lastUserAppliedInstrumentTwoState = state.LastUserAppliedConfigurations[Instrument.Two];
        var lastUserAppliedInstrumentThreeState = state.LastUserAppliedConfigurations[Instrument.Three];

        instrumentOneState.MinNote.Should().Be(Notes.C4);
        instrumentOneState.MaxNote.Should().Be(Notes.C5);
        instrumentTwoState.MinNote.Should().Be(Notes.C5);
        instrumentTwoState.MaxNote.Should().Be(Notes.C6);
        instrumentThreeState.MinNote.Should().Be(Notes.C6);
        instrumentThreeState.MaxNote.Should().Be(Notes.C7);

        lastUserAppliedInstrumentOneState.MinNote.Should().Be(Notes.C4);
        lastUserAppliedInstrumentOneState.MaxNote.Should().Be(Notes.C5);
        lastUserAppliedInstrumentTwoState.MinNote.Should().Be(Notes.C5);
        lastUserAppliedInstrumentTwoState.MaxNote.Should().Be(Notes.C6);
        lastUserAppliedInstrumentThreeState.MinNote.Should().Be(Notes.C6);
        lastUserAppliedInstrumentThreeState.MaxNote.Should().Be(Notes.C7);
    }

    [Test]
    public void ReduceUpdateConfigurationStatus_updates_instrument_configurations_as_expected()
    {
        // arrange
        var state = new InstrumentConfigurationState();

        // act
        state = InstrumentConfigurationReducers.ReduceUpdateConfigurationStatus(
            state,
            new UpdateInstrumentConfigurationStatus(Instrument.One, ConfigurationStatus.Disabled)
        );

        state = InstrumentConfigurationReducers.ReduceUpdateConfigurationStatus(
            state,
            new UpdateInstrumentConfigurationStatus(Instrument.Two, ConfigurationStatus.Locked)
        );

        state = InstrumentConfigurationReducers.ReduceUpdateConfigurationStatus(
            state,
            new UpdateInstrumentConfigurationStatus(Instrument.Three, ConfigurationStatus.Enabled)
        );

        // assert
        state.Configurations.Should().ContainKeys(Instrument.One, Instrument.Two, Instrument.Three);

        var instrumentOneState = state[Instrument.One]!;
        var instrumentTwoState = state[Instrument.Two]!;
        var instrumentThreeState = state[Instrument.Three]!;

        var lastUserAppliedInstrumentOneState = state.LastUserAppliedConfigurations[Instrument.One];
        var lastUserAppliedInstrumentTwoState = state.LastUserAppliedConfigurations[Instrument.Two];
        var lastUserAppliedInstrumentThreeState = state.LastUserAppliedConfigurations[Instrument.Three];

        instrumentOneState.Status.Should().Be(ConfigurationStatus.Disabled);
        instrumentTwoState.Status.Should().Be(ConfigurationStatus.Locked);
        instrumentThreeState.Status.Should().Be(ConfigurationStatus.Enabled);

        lastUserAppliedInstrumentOneState.Status.Should().Be(ConfigurationStatus.Disabled);
        lastUserAppliedInstrumentTwoState.Status.Should().Be(ConfigurationStatus.Locked);
        lastUserAppliedInstrumentThreeState.Status.Should().Be(ConfigurationStatus.Enabled);
    }
}
