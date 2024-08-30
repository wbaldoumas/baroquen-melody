using BaroquenMelody.Library.Compositions.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateInstrumentConfiguration(
    Instrument Instrument,
    Note MinNote,
    Note MaxNote,
    GeneralMidi2Program MidiProgram,
    bool IsEnabled,
    bool IsUserApplied
);
