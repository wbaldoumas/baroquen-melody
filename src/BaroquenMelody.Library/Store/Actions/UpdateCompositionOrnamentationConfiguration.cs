using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateCompositionOrnamentationConfiguration(OrnamentationType OrnamentationType, ConfigurationStatus Status, int Probability);
