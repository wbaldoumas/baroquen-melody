namespace BaroquenMelody.Library.Compositions;

/// <summary>
///     The composition.
/// </summary>
/// <param name="Measures"> The measures which make up the composition. </param>
internal sealed record Composition(
    IList<Measure> Measures
);
