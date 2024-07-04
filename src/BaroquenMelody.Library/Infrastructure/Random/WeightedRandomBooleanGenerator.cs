namespace BaroquenMelody.Library.Infrastructure.Random;

/// <summary>
///     Generates random boolean values. Can pass a probability to increase or decrease the likelihood of a true value.
/// </summary>
internal static class WeightedRandomBooleanGenerator
{
    private const int Threshold = 100;

    /// <summary>
    ///     Generates a random boolean value.
    /// </summary>
    /// <param name="weight">The probability of generating a true value. Default is 50.</param>
    /// <returns>A random boolean value.</returns>
    public static bool Generate(int weight = 50) => weight > ThreadLocalRandom.Next(Threshold);
}
