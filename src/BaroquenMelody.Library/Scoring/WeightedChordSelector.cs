using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Scoring;

/// <inheritdoc cref="IChordSelector"/>
/// <remarks>
///     Ranks the rule-passing candidate chords by their aggregate <see cref="IScoringRule"/> penalty and samples one
///     from a softmax (Boltzmann) distribution: each candidate is chosen with probability proportional to
///     <c>exp(-penalty / temperature)</c>. Lower-penalty (more idiomatic) candidates are favored, but higher-penalty
///     ones still occur, so the music keeps moving instead of freezing on the single smoothest option every beat.
///     <see cref="Temperature"/> is the knob: lower values approach a strict minimum-penalty pick (smoother, more
///     conservative); higher values approach a uniform random pick (more adventurous). All randomness flows through the
///     injected <see cref="IRandomProvider"/>, so selection stays deterministic under a seed. With no enabled scoring
///     rules every penalty is zero, so the distribution is uniform — equivalent to the legacy random pick.
/// </remarks>
internal sealed class WeightedChordSelector(IScoringRule scoringRule, IRandomProvider randomProvider) : IChordSelector
{
    /// <summary>
    ///     Controls how sharply lower-penalty candidates are favored. Lower → smoother and more conservative; higher →
    ///     more varied and adventurous. Tuned by ear at the energetic end of the usable range: even a noticeably busier
    ///     chord lands roughly half as often as the smoothest one, favoring motion and excitement while idiomatic
    ///     choices still carry the most weight (well short of the uniform, aimless-wandering regime).
    /// </summary>
    private const double Temperature = 20d;

    public BaroquenChord? SelectNextChord(IReadOnlyList<BaroquenChord> precedingChords, IEnumerable<BaroquenChord> candidateChords)
    {
        var candidates = candidateChords as IReadOnlyList<BaroquenChord> ?? candidateChords.ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        var penalties = new double[candidates.Count];
        var minPenalty = double.MaxValue;

        for (var index = 0; index < candidates.Count; ++index)
        {
            penalties[index] = scoringRule.Score(precedingChords, candidates[index]);
            minPenalty = Math.Min(minPenalty, penalties[index]);
        }

        var weights = new double[candidates.Count];
        var totalWeight = 0d;

        for (var index = 0; index < candidates.Count; ++index)
        {
            // Shifting by the minimum penalty keeps every exponent in (-inf, 0] (the best candidate weighs 1) without
            // changing the normalized distribution.
            weights[index] = Math.Exp(-(penalties[index] - minPenalty) / Temperature);
            totalWeight += weights[index];
        }

        var target = randomProvider.Next() / (double)int.MaxValue * totalWeight;
        var cumulativeWeight = 0d;

        for (var index = 0; index < candidates.Count; ++index)
        {
            cumulativeWeight += weights[index];

            if (target < cumulativeWeight)
            {
                return candidates[index];
            }
        }

        return candidates[^1];
    }
}
