using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AggregateCompositionRule(IEnumerable<ICompositionRule> compositionRules) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord) =>
        compositionRules.All(compositionRule => compositionRule.Evaluate(precedingChords, nextChord));
}
