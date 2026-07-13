namespace BaroquenMelody.Library.Rhythm;

/// <summary>
///     Decides which beats of the composition body hold the preceding harmony instead of receiving a freshly
///     composed chord, shaping the composition's harmonic rhythm.
/// </summary>
internal interface IHarmonicRhythmScheduler
{
    /// <summary>
    ///     Determine whether the given beat should hold the preceding beat's harmony.
    /// </summary>
    /// <param name="measureIndex">The zero-based index of the measure within the composition body.</param>
    /// <param name="beatIndex">The zero-based index of the beat within its measure.</param>
    /// <returns>Whether the beat should hold the preceding harmony.</returns>
    bool ShouldHoldBeat(int measureIndex, int beatIndex);
}
