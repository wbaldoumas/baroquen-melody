namespace BaroquenMelody.Infrastructure.Random;

/// <summary>
///     An <see cref="IRandomProvider"/> backed by a seeded <see cref="System.Random"/>, producing a deterministic
///     sequence for a given seed. Intended for single-threaded, deterministic use such as tests.
/// </summary>
/// <param name="seed">The seed that determines the sequence of generated values.</param>
public sealed class SeededRandomProvider(int seed) : IRandomProvider
{
    private readonly System.Random _random = new(seed);

    /// <inheritdoc/>
    public int Next() => _random.Next();

    /// <inheritdoc/>
    public int Next(int maxValue) => _random.Next(maxValue);

    /// <inheritdoc/>
    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);
}
