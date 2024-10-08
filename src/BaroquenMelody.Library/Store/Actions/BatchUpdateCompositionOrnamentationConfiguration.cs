using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Ornamentation.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record BatchUpdateCompositionOrnamentationConfiguration(IDictionary<OrnamentationType, OrnamentationConfiguration> Configurations);
