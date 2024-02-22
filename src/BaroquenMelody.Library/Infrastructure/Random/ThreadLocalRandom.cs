namespace BaroquenMelody.Library.Infrastructure.Random;

/// <summary>
///    A thread-local random number generator.
/// </summary>
internal static class ThreadLocalRandom
{
    private static readonly ThreadLocal<System.Random> ThreadLocalRandomStorage = new(
        () => new System.Random(Interlocked.Increment(ref _seed))
    );

    private static int _seed = Environment.TickCount;

    /// <summary>
    ///    Gets the thread-local random number generator.
    /// </summary>
    public static System.Random Instance => ThreadLocalRandomStorage.Value!;

    /// <summary>
    ///    Generates a random int within the specified range.
    /// </summary>
    /// <param name="maxValue">The upper bound of the range.</param>
    /// <returns>The random int.</returns>
    public static int Next(int maxValue) => Instance.Next(maxValue);
}
