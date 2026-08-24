using BaroquenMelody.Infrastructure.Random;

namespace BaroquenMelody.Library.Tests.TestData;

/// <summary>
///     The two independent seeded streams a seeded composition needs: the composition's shared draw stream, and the
///     ornamentation processor shuffle's own stream. The shuffle stream is salted so the two never replay the same
///     sequence, and it is kept apart from the shared stream so the shuffle itself consumes none of the composition's draws.
/// </summary>
internal static class SeededRandomProviders
{
    private const int ProcessorShuffleSalt = 0x5EED;

    public static SeededRandomProvider ForComposition(int seed) => new(seed);

    public static SeededRandomProvider ForProcessorShuffle(int seed) => new(seed ^ ProcessorShuffleSalt);
}
