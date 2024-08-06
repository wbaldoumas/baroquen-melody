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
    ///    Generates a random <see cref="int"/> within the specified range.
    /// </summary>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <returns>The random <inheritdoc cref="int"/>.</returns>
    public static int Next(int maxValue) => Instance.Next(maxValue);

    /// <summary>
    ///   Generates a random <see cref="int"/>>.
    /// </summary>
    /// <returns>The random <see cref="int"/>.</returns>
    public static int Next() => Instance.Next();
}
