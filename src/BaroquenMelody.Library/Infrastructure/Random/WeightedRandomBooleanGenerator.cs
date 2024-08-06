namespace BaroquenMelody.Library.Infrastructure.Random;

/// <inheritdoc cref="IWeightedRandomBooleanGenerator"/>
internal sealed class WeightedRandomBooleanGenerator : IWeightedRandomBooleanGenerator
{
    private const int Threshold = 100;

    public bool IsTrue(int weight = 50) => weight > ThreadLocalRandom.Next(Threshold);
}
