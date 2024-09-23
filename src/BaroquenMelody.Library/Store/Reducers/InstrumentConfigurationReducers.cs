using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
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
            [action.Instrument] = new(action.Instrument, action.MinNote, action.MaxNote, action.MidiProgram, action.Status)
        };

        if (!action.IsUserApplied)
        {
            return state with { Configurations = configurations };
        }

        var lastUserAppliedConfigurations = new Dictionary<Instrument, InstrumentConfiguration>(state.LastUserAppliedConfigurations)
        {
            [action.Instrument] = new(action.Instrument, action.MinNote, action.MaxNote, action.MidiProgram, action.Status)
        };

        return new InstrumentConfigurationState(configurations, lastUserAppliedConfigurations);
    }
}
