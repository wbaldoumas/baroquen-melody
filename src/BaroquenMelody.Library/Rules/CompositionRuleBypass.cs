using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Rules;

/// <summary>
///     A composition rule that allows for bypass of the underlying composition rule.
/// </summary>
internal sealed class CompositionRuleBypass(ICompositionRule rule, IWeightedRandomBooleanGenerator weightedRandomBooleanGenerator, int strictness) : ICompositionRule
{
    private const int Threshold = 100;

    public bool Evaluate(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord) =>
        weightedRandomBooleanGenerator.IsTrue(Threshold - strictness) || rule.Evaluate(precedingChords, nextChord);
}
