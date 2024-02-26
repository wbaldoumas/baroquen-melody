using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Evaluations;

/// <summary>
///     Represents a composition instructor which can evaluate a composition composed by a composition student.
/// </summary>
internal interface ICompositionInstructor
{
    /// <summary>
    ///     Evaluates the given composition, marking which notes are detrimental and returning the evaluated composition.
    /// </summary>
    /// <param name="composition">The composition to be evaluated.</param>
    /// <returns>The evaluated composition.</returns>
    Composition EvaluateComposition(Composition composition);
}
