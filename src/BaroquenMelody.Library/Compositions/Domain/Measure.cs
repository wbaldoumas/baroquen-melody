using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Domain;

internal sealed record Measure(IEnumerable<Beat> Beats, Meter Meter);
