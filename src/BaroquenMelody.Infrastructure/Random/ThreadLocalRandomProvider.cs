namespace BaroquenMelody.Infrastructure.Random;

/// <summary>
///     The default <see cref="IRandomProvider"/>, delegating to <see cref="ThreadLocalRandom"/> so production
///     randomness preserves its existing thread-safe, non-deterministic behavior and distribution.
/// </summary>
public sealed class ThreadLocalRandomProvider : IRandomProvider
{
    /// <inheritdoc/>
    public int Next() => ThreadLocalRandom.Next();

    /// <inheritdoc/>
    public int Next(int maxValue) => ThreadLocalRandom.Next(maxValue);

    /// <inheritdoc/>
    public int Next(int minValue, int maxValue) => ThreadLocalRandom.Next(minValue, maxValue);
}
