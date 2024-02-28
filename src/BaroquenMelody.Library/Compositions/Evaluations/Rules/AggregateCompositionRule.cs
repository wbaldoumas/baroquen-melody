using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

/// <inheritdoc cref="ICompositionRule"/>
internal sealed class AggregateCompositionRule(IEnumerable<ICompositionRule> compositionRules) : ICompositionRule
{
    public bool ValidateChordProgression(ContextualizedChord currentChord, ContextualizedChord nextChord) =>
        compositionRules.All(compositionRule => compositionRule.ValidateChordProgression(currentChord, nextChord));
}
