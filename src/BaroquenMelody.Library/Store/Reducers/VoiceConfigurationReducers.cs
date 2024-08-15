using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Reducers;

public static class VoiceConfigurationReducers
{
    [ReducerMethod]
    public static VoiceConfigurationState ReduceUpdateVoiceConfiguration(VoiceConfigurationState state, UpdateVoiceConfiguration action)
    {
        var configurations = new Dictionary<Voice, VoiceConfiguration>(state.Configurations)
        {
            [action.Voice] = new(action.Voice, action.MinNote, action.MaxNote, action.Instrument, action.IsEnabled)
        };

        return new VoiceConfigurationState(configurations);
    }
}
