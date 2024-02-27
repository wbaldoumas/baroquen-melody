using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;

namespace BaroquenMelody.Library.Compositions.Evaluations;

/// <summary>
///     Represents a composition instructor which can evaluate a composition composed by a composition student.
/// </summary>
/// <param name="compositionRules"> The composition rules that the composition instructor will use to evaluate the composition. </param>
internal sealed class CompositionInstructor(IEnumerable<ICompositionRule> compositionRules) : ICompositionInstructor
{
    public Composition EvaluateComposition(Composition composition) => compositionRules.Aggregate(
        composition,
        (current, compositionRule) => compositionRule.EvaluateComposition(current)
    );
}
