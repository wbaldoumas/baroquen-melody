using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateCompositionOrnamentationConfiguration(OrnamentationType OrnamentationType, bool IsEnabled, int Probability);
