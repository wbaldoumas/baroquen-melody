using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Viewpoints;

namespace BaroquenMelody.Library.Rules.Melodic;

/// <inheritdoc cref="ICompositionRule"/>
/// <remarks>
///     Projects the candidate chord onto the melodic viewpoint and requires the inner rule to pass for every
///     voice's line, letting a per-voice melodic rule participate in chord-level validation unchanged.
/// </remarks>
internal sealed class MelodicCompositionRuleAdapter(IMelodicCompositionRule melodicCompositionRule) : ICompositionRule
{
    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord)
    {
        foreach (var note in nextChord.Notes)
        {
            if (!melodicCompositionRule.Evaluate(new MelodicLine(precedingChords, note)))
            {
                return false;
            }
        }

        return true;
    }
}
