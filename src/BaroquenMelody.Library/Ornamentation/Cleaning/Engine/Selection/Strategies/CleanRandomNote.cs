using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection.Strategies;

internal sealed class CleanRandomNote(IWeightedRandomBooleanGenerator booleanGenerator) : IOrnamentationCleaningSelectorStrategy
{
    public BaroquenNote Select(BaroquenNote primaryNote, BaroquenNote secondaryNote) => booleanGenerator.IsTrue()
        ? primaryNote
        : secondaryNote;
}
