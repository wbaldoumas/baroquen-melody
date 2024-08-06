using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Rules;

/// <summary>
///     A composition rule to be checked against a chord progression in a composition.
/// </summary>
internal interface ICompositionRule
{
    /// <summary>
    ///   Checks if the given chords are valid according to the rule.
    /// </summary>
    /// <param name="precedingChords">The chords which precede the proposed next chord.</param>
    /// <param name="nextChord">The proposed next chord.</param>
    /// <returns>Whether the proposed next chord is valid according to the rule.</returns>
    bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord);
}
