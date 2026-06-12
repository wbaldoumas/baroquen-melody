using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Scoring;

/// <inheritdoc cref="IChordSelector"/>
/// <remarks>
///     Selects a minimum-penalty candidate according to the configured <see cref="IScoringRule"/>, breaking ties
///     uniformly at random via the injected <see cref="IRandomProvider"/> (so selection stays deterministic under
///     a seed). Penalties are integer-valued sums (see <see cref="IScoringRule"/>), so exact equality identifies
///     ties. With no enabled scoring rules every candidate ties at zero and selection degrades to the legacy
///     uniform random pick over all candidates.
/// </remarks>
internal sealed class WeightedChordSelector(IScoringRule scoringRule, IRandomProvider randomProvider) : IChordSelector
{
    public BaroquenChord? SelectNextChord(IReadOnlyList<BaroquenChord> precedingChords, IEnumerable<BaroquenChord> candidateChords)
    {
        var bestPenalty = double.MaxValue;
        var bestChords = new List<BaroquenChord>();

        foreach (var candidateChord in candidateChords)
        {
            var penalty = scoringRule.Score(precedingChords, candidateChord);

            if (penalty < bestPenalty)
            {
                bestPenalty = penalty;
                bestChords.Clear();
                bestChords.Add(candidateChord);
            }
            else if (penalty == bestPenalty)
            {
                bestChords.Add(candidateChord);
            }
        }

        return bestChords.MinByRandom(randomProvider);
    }
}
