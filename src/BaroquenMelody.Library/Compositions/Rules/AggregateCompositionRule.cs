using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AggregateCompositionRule(List<ICompositionRule> compositionRules) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        foreach (var compositionRule in compositionRules)
        {
            if (!compositionRule.Evaluate(precedingChords, nextChord))
            {
                return false;
            }
        }

        return true;
    }
}
