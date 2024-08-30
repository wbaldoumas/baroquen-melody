using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
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
            new UpdateInstrumentConfiguration(Instrument.One, Notes.C4, Notes.C5, GeneralMidi2Program.Accordion, IsEnabled: true, IsUserApplied: true)
        );

        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(
            state,
            new UpdateInstrumentConfiguration(Instrument.Two, Notes.C5, Notes.C6, GeneralMidi2Program.Banjo, IsEnabled: true, IsUserApplied: true)
        );

        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(
            state,
            new UpdateInstrumentConfiguration(Instrument.Three, Notes.C6, Notes.C7, GeneralMidi2Program.Celesta, IsEnabled: true, IsUserApplied: true)
        );

        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(
            state,
            new UpdateInstrumentConfiguration(Instrument.One, Notes.C7, Notes.C8, GeneralMidi2Program.Dulcimer, IsEnabled: false, IsUserApplied: false)
        );

        // assert
        state.Configurations.Should().ContainKeys(Instrument.One, Instrument.Two, Instrument.Three);

        var instrumentOneState = state[Instrument.One]!;
        var instrumentTwoState = state[Instrument.Two]!;
        var instrumentThreeState = state[Instrument.Three]!;

        instrumentOneState.MinNote.Should().Be(Notes.C7);
        instrumentOneState.MaxNote.Should().Be(Notes.C8);
        instrumentOneState.MidiProgram.Should().Be(GeneralMidi2Program.Dulcimer);
        instrumentOneState.IsEnabled.Should().BeFalse();

        instrumentTwoState.MinNote.Should().Be(Notes.C5);
        instrumentTwoState.MaxNote.Should().Be(Notes.C6);
        instrumentTwoState.MidiProgram.Should().Be(GeneralMidi2Program.Banjo);
        instrumentTwoState.IsEnabled.Should().BeTrue();

        instrumentThreeState.MinNote.Should().Be(Notes.C6);
        instrumentThreeState.MaxNote.Should().Be(Notes.C7);
        instrumentThreeState.MidiProgram.Should().Be(GeneralMidi2Program.Celesta);
        instrumentThreeState.IsEnabled.Should().BeTrue();

        var lastUserAppliedInstrumentOneState = state.LastUserAppliedConfigurations[Instrument.One];
        var lastUserAppliedInstrumentTwoState = state.LastUserAppliedConfigurations[Instrument.Two];
        var lastUserAppliedInstrumentThreeState = state.LastUserAppliedConfigurations[Instrument.Three];

        lastUserAppliedInstrumentOneState.MinNote.Should().Be(Notes.C4);
        lastUserAppliedInstrumentOneState.MaxNote.Should().Be(Notes.C5);
        lastUserAppliedInstrumentOneState.MidiProgram.Should().Be(GeneralMidi2Program.Accordion);
        lastUserAppliedInstrumentOneState.IsEnabled.Should().BeTrue();

        lastUserAppliedInstrumentTwoState.MinNote.Should().Be(Notes.C5);
        lastUserAppliedInstrumentTwoState.MaxNote.Should().Be(Notes.C6);
        lastUserAppliedInstrumentTwoState.MidiProgram.Should().Be(GeneralMidi2Program.Banjo);
        lastUserAppliedInstrumentTwoState.IsEnabled.Should().BeTrue();

        lastUserAppliedInstrumentThreeState.MinNote.Should().Be(Notes.C6);
        lastUserAppliedInstrumentThreeState.MaxNote.Should().Be(Notes.C7);
        lastUserAppliedInstrumentThreeState.MidiProgram.Should().Be(GeneralMidi2Program.Celesta);
        lastUserAppliedInstrumentThreeState.IsEnabled.Should().BeTrue();

        state.EnabledConfigurations.Should().BeEquivalentTo(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.Two, Notes.C5, Notes.C6, GeneralMidi2Program.Banjo),
                new(Instrument.Three, Notes.C6, Notes.C7, GeneralMidi2Program.Celesta)
            }
        );
    }
}
