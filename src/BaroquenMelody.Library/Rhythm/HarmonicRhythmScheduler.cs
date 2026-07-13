using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Rhythm;

/// <inheritdoc cref="IHarmonicRhythmScheduler"/>
/// <remarks>
///     Deterministic by design (no randomness): phrase-interior measures hold each harmony across two beats,
///     so harmonies change only on the strong and medium beats, while the measure before each phrase seam
///     returns to per-beat harmonic motion, accelerating into the cadence. Phrase seams follow
///     <see cref="PhrasingConfiguration.MinPhraseLength"/>, matching where the phraser attempts repetitions.
/// </remarks>
internal sealed class HarmonicRhythmScheduler(CompositionConfiguration compositionConfiguration) : IHarmonicRhythmScheduler
{
    private readonly HarmonicRhythmConfiguration _harmonicRhythmConfiguration =
        compositionConfiguration.HarmonicRhythmConfiguration ?? HarmonicRhythmConfiguration.Default;

    public bool ShouldHoldBeat(int measureIndex, int beatIndex)
    {
        if (!_harmonicRhythmConfiguration.Enabled)
        {
            return false;
        }

        var isAccelerationMeasure = (measureIndex + 1) % compositionConfiguration.PhrasingConfiguration.MinPhraseLength == 0;

        return !isAccelerationMeasure && beatIndex % 2 == 1;
    }
}
