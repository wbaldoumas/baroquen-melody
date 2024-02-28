using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Evaluations.Rules;

/// <summary>
///     A composition rule to be checked against a chord progression in a composition.
/// </summary>
internal interface ICompositionRule
{
    /// <summary>
    ///    Checks if the given chord progression from the current chord to the next chord is valid.
    /// </summary>
    /// <param name="currentChord">The current chord in the progression.</param>
    /// <param name="nextChord">The next chord in the progression.</param>
    /// <returns>True if the chord progression is valid, false otherwise.</returns>
    bool ValidateChordProgression(ContextualizedChord currentChord, ContextualizedChord nextChord);
}
