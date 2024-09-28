using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Infrastructure.Random;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection.Strategies;

internal sealed class CleanRandomNote(IWeightedRandomBooleanGenerator booleanGenerator) : IOrnamentationCleaningSelectorStrategy
{
    public BaroquenNote Select(BaroquenNote primaryNote, BaroquenNote secondaryNote) => booleanGenerator.IsTrue()
        ? primaryNote
        : secondaryNote;
}
