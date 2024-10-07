using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateInstrumentConfigurationStatus(Instrument Instrument, ConfigurationStatus Status);
