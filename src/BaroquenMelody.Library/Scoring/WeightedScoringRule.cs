using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Scoring;

/// <inheritdoc cref="IScoringRule"/>
/// <remarks>
///     Scales an inner scoring rule's penalty by a configured weight, allowing preferences to be balanced
///     against each other when their penalties are aggregated.
/// </remarks>
internal sealed class WeightedScoringRule(IScoringRule scoringRule, int weight) : IScoringRule
{
    public double Score(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord) => weight * scoringRule.Score(precedingChords, nextChord);
}
