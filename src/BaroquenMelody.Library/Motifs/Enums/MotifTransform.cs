namespace BaroquenMelody.Library.Motifs.Enums;

/// <summary>
///     A development applied to a theme's motif before it is restated. Pitch-preserving transforms (Invert, Retrograde)
///     keep the gesture count and durations and so tile the beat grid safely; the duration- and length-changing
///     transforms are reserved for a later iteration that can re-place gestures within a measure.
/// </summary>
public enum MotifTransform : byte
{
    /// <summary> The motif unchanged. A no-op oracle; excluded from the default repertoire (no audible development). </summary>
    Identity = 0,

    /// <summary> The motif mirrored: every scale-step delta is negated. Pitch-preserving and grid-safe. </summary>
    Invert,

    /// <summary> The motif reversed: gestures play back-to-front. Pitch-preserving and grid-safe. </summary>
    Retrograde,

    /// <summary> The motif lengthened: each duration is multiplied. Duration-changing, so grid-unsafe (reserved). </summary>
    Augment,

    /// <summary> The motif shortened: each duration is divided. Duration-changing, so grid-unsafe (reserved). </summary>
    Diminish,

    /// <summary> A contiguous slice of the motif. Length-changing, so grid-unsafe (reserved). </summary>
    Fragment
}
