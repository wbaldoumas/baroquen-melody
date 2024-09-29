namespace BaroquenMelody.Infrastructure.Random;

/// <inheritdoc cref="IWeightedRandomBooleanGenerator"/>
public sealed class WeightedRandomBooleanGenerator : IWeightedRandomBooleanGenerator
{
    private const int Threshold = 100;

    public bool IsTrue(int weight = 50) => weight > ThreadLocalRandom.Next(Threshold);
}
