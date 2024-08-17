using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Store.Actions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

internal sealed class InstrumentConfigurationService(IDispatcher dispatcher) : IInstrumentConfigurationService
{
    private static readonly FrozenSet<Instrument> _configurableInstruments = new[]
    {
        Instrument.One,
        Instrument.Two,
        Instrument.Three,
        Instrument.Four
    }.ToFrozenSet();

    public IEnumerable<Instrument> ConfigurableInstruments => _configurableInstruments;

    public void ConfigureDefaults()
    {
        dispatcher.Dispatch(new UpdateInstrumentConfiguration(Instrument.One, Notes.C5, Notes.E6, GeneralMidi2Program.AcousticGuitarNylon, true));
        dispatcher.Dispatch(new UpdateInstrumentConfiguration(Instrument.Two, Notes.G3, Notes.B4, GeneralMidi2Program.AcousticGuitarNylon, true));
        dispatcher.Dispatch(new UpdateInstrumentConfiguration(Instrument.Three, Notes.F3, Notes.A4, GeneralMidi2Program.AcousticGuitarNylon, true));
        dispatcher.Dispatch(new UpdateInstrumentConfiguration(Instrument.Four, Notes.C2, Notes.E3, GeneralMidi2Program.AcousticGuitarNylon, false));
    }
}
