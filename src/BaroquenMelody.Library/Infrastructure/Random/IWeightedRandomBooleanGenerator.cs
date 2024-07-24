namespace BaroquenMelody.Library.Infrastructure.Random;

/// <summary>
///     Generates random boolean values. Can pass a weight probability to increase or decrease the likelihood of a true value.
/// </summary>
public interface IWeightedRandomBooleanGenerator
{
    /// <summary>
    ///     Generates a random boolean value.
    /// </summary>
    /// <param name="weight">The probability of generating a true value. Default is 50.</param>
    /// <returns>A random boolean value.</returns>
    bool IsTrue(int weight = 50);
}
