using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;

namespace BaroquenMelody.Library.Compositions.Evaluations;

internal sealed class CompositionInstructor(IEnumerable<ICompositionRule> compositionRules) : ICompositionInstructor
{
    public Composition EvaluateComposition(Composition composition) => compositionRules.Aggregate(
        composition,
        (current, compositionRule) => compositionRule.EvaluateComposition(current)
    );
}
