using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Viewpoints;

namespace BaroquenMelody.Library.Scoring.Melodic;

/// <inheritdoc cref="IScoringRule"/>
/// <remarks>
///     Projects the candidate chord onto the melodic viewpoint and sums the inner rule's penalty over every voice,
///     letting a per-voice melodic preference participate in chord-level scoring unchanged.
/// </remarks>
internal sealed class MelodicScoringRuleAdapter(IMelodicScoringRule melodicScoringRule) : IScoringRule
{
    public double Score(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        var penalty = 0d;

        foreach (var note in nextChord.Notes)
        {
            penalty += melodicScoringRule.Score(new MelodicLine(precedingChords, note));
        }

        return penalty;
    }
}
