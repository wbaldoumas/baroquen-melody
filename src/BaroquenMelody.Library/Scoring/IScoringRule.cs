using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Scoring;

/// <summary>
///     Represents a soft preference used to rank candidate chords which have already passed the hard
///     <see cref="Rules.ICompositionRule"/> pre-filters. Lower is better: zero means the preference is
///     fully satisfied, and positive values are penalties.
/// </summary>
/// <remarks>
///     Implementations must be pure and deterministic, and should return integer-valued penalties so that
///     weighted sums of penalties compare exactly (candidate ties are detected with exact equality).
/// </remarks>
internal interface IScoringRule
{
    /// <summary>
    ///     Scores the given chord in the context of the preceding chords.
    /// </summary>
    /// <param name="precedingChords">The chords which precede the proposed next chord.</param>
    /// <param name="nextChord">The proposed next chord.</param>
    /// <returns>The penalty for the proposed next chord. Zero is ideal; larger is worse.</returns>
    public double Score(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord);
}
