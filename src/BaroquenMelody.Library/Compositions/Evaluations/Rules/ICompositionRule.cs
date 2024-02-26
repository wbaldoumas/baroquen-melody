using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

/// <summary>
///     A composition rule to be checked against a composition.
/// </summary>
internal interface ICompositionRule
{
    /// <summary>
    ///     Evaluates the given composition for any violations of the rule.
    /// </summary>
    /// <param name="composition">The composition to be evaluated.</param>
    /// <returns>The evaluated composition.</returns>
    Composition EvaluateComposition(Composition composition);
}
