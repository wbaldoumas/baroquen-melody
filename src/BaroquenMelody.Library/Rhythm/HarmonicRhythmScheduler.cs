using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Rhythm;

/// <inheritdoc cref="IHarmonicRhythmScheduler"/>
/// <remarks>
///     Deterministic by design (no randomness): phrase-interior measures hold each harmony across two beats,
///     so harmonies change only on the strong and medium beats, while phrase-seam measures return to per-beat
///     harmonic motion. Seam measures follow <see cref="PhrasingConfiguration.MinPhraseLength"/> and are the
///     measures the phraser finalizes when it attempts repetitions, so composing them fresh both accelerates
///     the harmonic rhythm into the seam and lets the seam's final two chords form a real cadence.
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

        var isSeamMeasure = measureIndex % compositionConfiguration.PhrasingConfiguration.MinPhraseLength == 0;

        return !isSeamMeasure && beatIndex % 2 == 1;
    }
}
