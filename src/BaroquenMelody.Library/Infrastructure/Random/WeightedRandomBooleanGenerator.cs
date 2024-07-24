namespace BaroquenMelody.Library.Infrastructure.Random;

/// <summary>
///     Generates random boolean values. Can pass a probability to increase or decrease the likelihood of a true value.
/// </summary>
internal sealed class WeightedRandomBooleanGenerator : IWeightedRandomBooleanGenerator
{
    private const int Threshold = 100;

    /// <summary>
    ///     Generates a random boolean value.
    /// </summary>
    /// <param name="weight">The probability of generating a true value. Default is 50.</param>
    /// <returns>A random boolean value.</returns>
    public bool IsTrue(int weight = 50) => weight > ThreadLocalRandom.Next(Threshold);
}
