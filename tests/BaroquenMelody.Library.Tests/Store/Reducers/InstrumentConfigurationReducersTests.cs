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
        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(state, new UpdateInstrumentConfiguration(Instrument.One, Notes.C4, Notes.C5, GeneralMidi2Program.Accordion, true));
        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(state, new UpdateInstrumentConfiguration(Instrument.Two, Notes.C5, Notes.C6, GeneralMidi2Program.Banjo, true));
        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(state, new UpdateInstrumentConfiguration(Instrument.Three, Notes.C6, Notes.C7, GeneralMidi2Program.Celesta, true));
        state = InstrumentConfigurationReducers.ReduceUpdateInstrumentConfiguration(state, new UpdateInstrumentConfiguration(Instrument.One, Notes.C7, Notes.C8, GeneralMidi2Program.Dulcimer, false));

        // assert
        state.Configurations.Should().ContainKeys(Instrument.One, Instrument.Two, Instrument.Three);

        state[Instrument.One]!.MinNote.Should().Be(Notes.C7);
        state[Instrument.One]!.MaxNote.Should().Be(Notes.C8);
        state[Instrument.One]!.MidiProgram.Should().Be(GeneralMidi2Program.Dulcimer);
        state[Instrument.One]!.IsEnabled.Should().BeFalse();
        state[Instrument.Two]!.MinNote.Should().Be(Notes.C5);
        state[Instrument.Two]!.MaxNote.Should().Be(Notes.C6);
        state[Instrument.Two]!.MidiProgram.Should().Be(GeneralMidi2Program.Banjo);
        state[Instrument.Two]!.IsEnabled.Should().BeTrue();
        state[Instrument.Three]!.MinNote.Should().Be(Notes.C6);
        state[Instrument.Three]!.MaxNote.Should().Be(Notes.C7);
        state[Instrument.Three]!.MidiProgram.Should().Be(GeneralMidi2Program.Celesta);
        state[Instrument.Three]!.IsEnabled.Should().BeTrue();

        state.Aggregate.Should().BeEquivalentTo(
            new HashSet<InstrumentConfiguration>
            {
                new(Instrument.Two, Notes.C5, Notes.C6, GeneralMidi2Program.Banjo),
                new(Instrument.Three, Notes.C6, Notes.C7, GeneralMidi2Program.Celesta)
            }
        );
    }
}
