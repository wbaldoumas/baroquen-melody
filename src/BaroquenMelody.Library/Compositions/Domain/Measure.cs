using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///    Represents a measure in a composition.
/// </summary>
/// <param name="Beats">The beats that make up the measure.</param>
/// <param name="Meter">The meter of the measure.</param>
internal sealed record Measure(IList<Beat> Beats, Meter Meter)
{
    public Measure(Measure measure)
    {
        Beats = measure.Beats.Select(static beat => new Beat(beat)).ToList();
        Meter = measure.Meter;
    }
}
