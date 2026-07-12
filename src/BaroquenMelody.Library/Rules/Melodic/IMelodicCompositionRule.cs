using BaroquenMelody.Library.Viewpoints;

namespace BaroquenMelody.Library.Rules.Melodic;

/// <summary>
///     A composition rule checked against a single voice's melodic line.
/// </summary>
/// <remarks>
///     A melodic rule participates in chord-level validation through <see cref="MelodicCompositionRuleAdapter"/>,
///     which requires every voice's line in the candidate chord to pass.
/// </remarks>
internal interface IMelodicCompositionRule
{
    /// <summary>
    ///     Checks if the proposed next note of the given melodic line is valid according to the rule.
    /// </summary>
    /// <param name="line">The melodic line one voice traces up to the proposed next note.</param>
    /// <returns>Whether the proposed next note is valid according to the rule.</returns>
    public bool Evaluate(in MelodicLine line);
}
