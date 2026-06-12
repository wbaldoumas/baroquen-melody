using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Scoring;

/// <inheritdoc cref="IChordSelector"/>
/// <remarks>
///     Selects a minimum-penalty candidate according to the configured <see cref="IScoringRule"/>, breaking ties
///     uniformly at random via the injected <see cref="IRandomProvider"/> (so selection stays deterministic under
///     a seed). Penalties are integer-valued sums (see <see cref="IScoringRule"/>), so exact equality identifies
///     ties. One tie-break key is drawn per candidate, interleaved with enumeration, which makes the
///     no-scoring-rules path draw-for-draw identical to the legacy <see cref="RandomProviderExtensions.MinByRandom{T}"/>
///     pick — even when the candidate stream itself consumes random draws while being enumerated (as the lazy
///     rule-bypass pipeline does for strictness values below 100).
/// </remarks>
internal sealed class WeightedChordSelector(IScoringRule scoringRule, IRandomProvider randomProvider) : IChordSelector
{
    public BaroquenChord? SelectNextChord(IReadOnlyList<BaroquenChord> precedingChords, IEnumerable<BaroquenChord> candidateChords)
    {
        BaroquenChord? bestChord = null;
        var bestPenalty = double.MaxValue;
        var bestTieBreaker = int.MaxValue;

        foreach (var candidateChord in candidateChords)
        {
            var penalty = scoringRule.Score(precedingChords, candidateChord);
            var tieBreaker = randomProvider.Next();

            if (penalty < bestPenalty || (penalty == bestPenalty && tieBreaker < bestTieBreaker))
            {
                bestChord = candidateChord;
                bestPenalty = penalty;
                bestTieBreaker = tieBreaker;
            }
        }

        return bestChord;
    }
}
