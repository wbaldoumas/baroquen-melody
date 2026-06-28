using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Scoring;

/// <inheritdoc cref="IScoringRule"/>
/// <remarks>
///     Sums the penalties of the given scoring rules. With no rules every candidate scores zero, which makes
///     ranking degrade gracefully to the legacy uniform random selection.
/// </remarks>
internal sealed class AggregateScoringRule(IReadOnlyList<IScoringRule> scoringRules) : IScoringRule
{
    public double Score(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var penalty = 0d;

        foreach (var scoringRule in scoringRules)
        {
            penalty += scoringRule.Score(precedingChords, nextChord);
        }

        return penalty;
    }
}
