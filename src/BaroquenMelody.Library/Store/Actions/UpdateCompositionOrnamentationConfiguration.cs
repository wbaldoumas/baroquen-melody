using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Infrastructure.Configuration.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateCompositionOrnamentationConfiguration(OrnamentationType OrnamentationType, ConfigurationStatus Status, int Probability);
