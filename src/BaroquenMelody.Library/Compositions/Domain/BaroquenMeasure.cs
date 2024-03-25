using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Domain;

internal sealed record BaroquenMeasure(IEnumerable<BaroquenBeat> Beats, Meter Meter);
