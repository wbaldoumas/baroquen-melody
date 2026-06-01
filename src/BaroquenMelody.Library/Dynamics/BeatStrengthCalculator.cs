namespace BaroquenMelody.Library.Dynamics;

/// <summary>
///     Determines the <see cref="MetricStrength"/> of a beat from its position within its (hyper)measure.
/// </summary>
/// <remarks>
///     The downbeat is always strong; a measure of four or more beats earns a secondary (medium) accent at its
///     mid-point; everything else is weak. This yields the canonical strong-weak-medium-weak pattern for the engine's
///     four-beat (hyper)measure while degrading gracefully for shorter or partial measures.
/// </remarks>
internal static class BeatStrengthCalculator
{
    private const int SecondaryAccentMinimumBeats = 4;

    public static MetricStrength Calculate(int beatIndexInMeasure, int beatsPerMeasure)
    {
        if (beatIndexInMeasure == 0)
        {
            return MetricStrength.Strong;
        }

        if (beatsPerMeasure >= SecondaryAccentMinimumBeats && beatIndexInMeasure == beatsPerMeasure / 2)
        {
            return MetricStrength.Medium;
        }

        return MetricStrength.Weak;
    }
}
