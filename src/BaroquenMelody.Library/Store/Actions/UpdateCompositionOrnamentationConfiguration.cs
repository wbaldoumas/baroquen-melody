using BaroquenMelody.Library.Compositions.Configurations.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateCompositionOrnamentationConfiguration(OrnamentationType OrnamentationType, ConfigurationStatus Status, int Probability);
