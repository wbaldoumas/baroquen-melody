namespace BaroquenMelody.Infrastructure.Random;

/// <inheritdoc cref="IWeightedRandomBooleanGenerator"/>
/// <param name="randomProvider">
///     The random provider used to generate values. When <see langword="null"/>, a <see cref="ThreadLocalRandomProvider"/>
///     is used so existing (non-injected) construction preserves the prior thread-local random behavior.
/// </param>
public sealed class WeightedRandomBooleanGenerator(IRandomProvider? randomProvider = null) : IWeightedRandomBooleanGenerator
{
    private const int Threshold = 100;

    private readonly IRandomProvider _randomProvider = randomProvider ?? new ThreadLocalRandomProvider();

    public bool IsTrue(int weight = 50) => weight > _randomProvider.Next(Threshold);
}
