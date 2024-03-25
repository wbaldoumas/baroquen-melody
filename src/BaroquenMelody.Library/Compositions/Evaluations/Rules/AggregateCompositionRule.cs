using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AggregateCompositionRule(IEnumerable<ICompositionRule> compositionRules) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<Chord> precedingChords, Chord nextChord) =>
        compositionRules.All(compositionRule => compositionRule.Evaluate(precedingChords, nextChord));
}
