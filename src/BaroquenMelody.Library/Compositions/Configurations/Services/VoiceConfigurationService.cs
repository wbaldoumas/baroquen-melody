using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

internal sealed class VoiceConfigurationService(IDispatcher dispatcher) : IVoiceConfigurationService
{
    private static readonly FrozenSet<Voice> _configurableVoices = new[]
    {
        Voice.One,
        Voice.Two,
        Voice.Three,
        Voice.Four
    }.ToFrozenSet();

    public IEnumerable<Voice> ConfigurableVoices => _configurableVoices;

    public void ConfigureDefaults()
    {
        dispatcher.Dispatch(new UpdateVoiceConfiguration(Voice.One, Notes.C5, Notes.E6, GeneralMidi2Program.AcousticGuitarNylon, true));
        dispatcher.Dispatch(new UpdateVoiceConfiguration(Voice.Two, Notes.G3, Notes.B4, GeneralMidi2Program.AcousticGuitarNylon, true));
        dispatcher.Dispatch(new UpdateVoiceConfiguration(Voice.Three, Notes.F3, Notes.A4, GeneralMidi2Program.AcousticGuitarNylon, true));
        dispatcher.Dispatch(new UpdateVoiceConfiguration(Voice.Four, Notes.C2, Notes.E3, GeneralMidi2Program.AcousticGuitarNylon, false));
    }
}
