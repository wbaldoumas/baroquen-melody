using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateInstrumentConfiguration(
    Instrument Instrument,
    Note MinNote,
    Note MaxNote,
    SevenBitNumber MinVelocity,
    SevenBitNumber MaxVelocity,
    GeneralMidi2Program MidiProgram,
    ConfigurationStatus Status,
    bool IsUserApplied
);
