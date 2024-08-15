using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Store.Actions;

public sealed record UpdateCompositionConfiguration(BaroquenScale Scale, Meter Meter, int CompositionLength);
