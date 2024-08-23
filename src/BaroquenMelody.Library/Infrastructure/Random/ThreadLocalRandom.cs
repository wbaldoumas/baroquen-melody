namespace BaroquenMelody.Library.Infrastructure.Random;

/// <summary>
///    A thread-local random number generator.
/// </summary>
internal static class ThreadLocalRandom
{
    private static readonly ThreadLocal<System.Random> ThreadLocalRandomStorage = new(() => new System.Random(Interlocked.Increment(ref _seed)));

    private static System.Random Instance => ThreadLocalRandomStorage.Value!;

    private static int _seed = Environment.TickCount;

    /// <summary>
    ///    Returns a non-negative <see cref="int"/> that is less than the specified <paramref name="maxValue"/>.
    /// </summary>
    /// <param name="maxValue">The upper (exclusive) bound of the range.</param>
    /// <returns>The random <inheritdoc cref="int"/>.</returns>
    public static int Next(int maxValue) => Instance.Next(maxValue);

    /// <summary>
    ///    Returns a non-negative <see cref="int"/> that is within the specified range.
    /// </summary>
    /// <param name="minValue">The lower (inclusive) bound of the range.</param>
    /// <param name="maxValue">The upper (exclusive) bound of the range.</param>
    /// <returns>The random <inheritdoc cref="int"/>.</returns>
    public static int Next(int minValue, int maxValue) => Instance.Next(minValue, maxValue);

    /// <summary>
    ///   Generates a random <see cref="int"/>>.
    /// </summary>
    /// <returns>The random <see cref="int"/>.</returns>
    public static int Next() => Instance.Next();
}
