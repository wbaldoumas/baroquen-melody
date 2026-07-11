using BaroquenMelody.Library.Viewpoints;

namespace BaroquenMelody.Library.Scoring.Melodic;

/// <summary>
///     Represents a soft preference over a single voice's melodic line, used to rank candidate chords which have
///     already passed the hard <see cref="Rules.ICompositionRule"/> pre-filters. Lower is better: zero means the
///     preference is fully satisfied, and positive values are penalties.
/// </summary>
/// <remarks>
///     Implementations must be pure and deterministic, and should return integer-valued penalties so that weighted
///     sums of penalties compare exactly. A melodic preference participates in chord-level scoring through
///     <see cref="MelodicScoringRuleAdapter"/>, which sums its penalties over every voice in the candidate chord.
/// </remarks>
internal interface IMelodicScoringRule
{
    /// <summary>
    ///     Scores the proposed next note of the given melodic line.
    /// </summary>
    /// <param name="line">The melodic line one voice traces up to the proposed next note.</param>
    /// <returns>The penalty for the proposed next note. Zero is ideal; larger is worse.</returns>
    public double Score(in MelodicLine line);
}
