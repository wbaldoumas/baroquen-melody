using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

/// <summary>
///     A composition rule to be checked against a chord progression in a composition.
/// </summary>
internal interface ICompositionRule
{
    /// <summary>
    ///    Checks if the given chords are valid according to the implemented rule.
    /// </summary>
    /// <param name="currentChord">The current chord in the progression.</param>
    /// <param name="nextChord">The next chord in the progression.</param>
    /// <returns>True if the chords are valid, False otherwise.</returns>
    bool Evaluate(ContextualizedChord currentChord, ContextualizedChord nextChord);
}
