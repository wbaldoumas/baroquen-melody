namespace BaroquenMelody.Infrastructure.Random;

/// <summary>
///     Provides random numbers through an injectable seam so composition-time randomness can be made deterministic under a seed.
/// </summary>
public interface IRandomProvider
{
    /// <summary>
    ///     Returns a non-negative random <see cref="int"/>.
    /// </summary>
    /// <returns>The random <see cref="int"/>.</returns>
    int Next();

    /// <summary>
    ///     Returns a non-negative random <see cref="int"/> that is less than the specified <paramref name="maxValue"/>.
    /// </summary>
    /// <param name="maxValue">The upper (exclusive) bound of the range.</param>
    /// <returns>The random <see cref="int"/>.</returns>
    int Next(int maxValue);

    /// <summary>
    ///     Returns a random <see cref="int"/> that is within the specified range.
    /// </summary>
    /// <param name="minValue">The lower (inclusive) bound of the range.</param>
    /// <param name="maxValue">The upper (exclusive) bound of the range.</param>
    /// <returns>The random <see cref="int"/>.</returns>
    int Next(int minValue, int maxValue);
}
