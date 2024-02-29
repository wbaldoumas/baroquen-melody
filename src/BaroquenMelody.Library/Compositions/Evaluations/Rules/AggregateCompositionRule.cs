using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AggregateCompositionRule(IEnumerable<ICompositionRule> compositionRules) : ICompositionRule
{
    public bool Evaluate(ContextualizedChord currentChord, ContextualizedChord nextChord) =>
        compositionRules.All(compositionRule => compositionRule.Evaluate(currentChord, nextChord));
}
