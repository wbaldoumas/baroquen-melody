namespace BaroquenMelody.Library.Compositions.Domain;

/// <summary>
///     A theme to be used in a composition.
/// </summary>
/// <param name="Exposition">The exposition, sometimes an opening fugue subject.</param>
/// <param name="Recapitulation">The recapitulation, the theme repeated with additional ornamentation.</param>
internal sealed record BaroquenTheme(List<Measure> Exposition, List<Measure> Recapitulation)
{
    public BaroquenTheme()
        : this([], [])
    {
    }
}
