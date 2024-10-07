using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Reducers;

public static class InstrumentConfigurationReducers
{
    [ReducerMethod]
    public static InstrumentConfigurationState ReduceUpdateInstrumentConfiguration(InstrumentConfigurationState state, UpdateInstrumentConfiguration action)
    {
        var configurations = new Dictionary<Instrument, InstrumentConfiguration>(state.Configurations)
        {
            [action.Instrument] = new(
                action.Instrument,
                action.MinNote,
                action.MaxNote,
                action.MinVelocity,
                action.MaxVelocity,
                action.MidiProgram,
                action.Status
            )
        };

        if (!action.IsUserApplied)
        {
            return state with { Configurations = configurations };
        }

        var lastUserAppliedConfigurations = new Dictionary<Instrument, InstrumentConfiguration>(state.LastUserAppliedConfigurations)
        {
            [action.Instrument] = new(
                action.Instrument,
                action.MinNote,
                action.MaxNote,
                action.MinVelocity,
                action.MaxVelocity,
                action.MidiProgram,
                action.Status
            )
        };

        return new InstrumentConfigurationState(configurations, lastUserAppliedConfigurations);
    }

    [ReducerMethod]
    public static InstrumentConfigurationState ReduceUpdateMidiInstrument(InstrumentConfigurationState state, UpdateMidiInstrument action)
    {
        var configurations = new Dictionary<Instrument, InstrumentConfiguration>(state.Configurations)
        {
            [action.Instrument] = state[action.Instrument]! with
            {
                MidiProgram = action.MidiInstrument
            }
        };

        var lastUserAppliedConfigurations = new Dictionary<Instrument, InstrumentConfiguration>(state.LastUserAppliedConfigurations)
        {
            [action.Instrument] = state.LastUserAppliedConfigurations[action.Instrument] with
            {
                MidiProgram = action.MidiInstrument
            }
        };

        return new InstrumentConfigurationState(configurations, lastUserAppliedConfigurations);
    }

    [ReducerMethod]
    public static InstrumentConfigurationState ReduceUpdateInstrumentVelocities(InstrumentConfigurationState state, UpdateInstrumentVelocities action)
    {
        var configurations = new Dictionary<Instrument, InstrumentConfiguration>(state.Configurations)
        {
            [action.Instrument] = state[action.Instrument]! with
            {
                MinVelocity = action.MinVelocity,
                MaxVelocity = action.MaxVelocity
            }
        };

        var lastUserAppliedConfigurations = new Dictionary<Instrument, InstrumentConfiguration>(state.LastUserAppliedConfigurations)
        {
            [action.Instrument] = state.LastUserAppliedConfigurations[action.Instrument] with
            {
                MinVelocity = action.MinVelocity,
                MaxVelocity = action.MaxVelocity
            }
        };

        return new InstrumentConfigurationState(configurations, lastUserAppliedConfigurations);
    }

    [ReducerMethod]
    public static InstrumentConfigurationState ReduceUpdateInstrumentTonalRange(InstrumentConfigurationState state, UpdateInstrumentTonalRange action)
    {
        var configurations = new Dictionary<Instrument, InstrumentConfiguration>(state.Configurations)
        {
            [action.Instrument] = state[action.Instrument]! with
            {
                MinNote = action.LowestPitchNote,
                MaxNote = action.HighestPitchNote
            }
        };

        var lastUserAppliedConfigurations = new Dictionary<Instrument, InstrumentConfiguration>(state.LastUserAppliedConfigurations)
        {
            [action.Instrument] = state.LastUserAppliedConfigurations[action.Instrument] with
            {
                MinNote = action.LowestPitchNote,
                MaxNote = action.HighestPitchNote
            }
        };

        return new InstrumentConfigurationState(configurations, lastUserAppliedConfigurations);
    }

    [ReducerMethod]
    public static InstrumentConfigurationState ReduceUpdateConfigurationStatus(InstrumentConfigurationState state, UpdateInstrumentConfigurationStatus action)
    {
        var configurations = new Dictionary<Instrument, InstrumentConfiguration>(state.Configurations)
        {
            [action.Instrument] = state[action.Instrument]! with
            {
                Status = action.Status
            }
        };

        var lastUserAppliedConfigurations = new Dictionary<Instrument, InstrumentConfiguration>(state.LastUserAppliedConfigurations)
        {
            [action.Instrument] = state.LastUserAppliedConfigurations[action.Instrument] with
            {
                Status = action.Status
            }
        };

        return new InstrumentConfigurationState(configurations, lastUserAppliedConfigurations);
    }
}
