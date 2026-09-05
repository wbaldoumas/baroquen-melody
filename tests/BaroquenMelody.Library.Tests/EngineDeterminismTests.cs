using BaroquenMelody.Library.Tests.TestData;
using CsCheck;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests;

/// <summary>
///     The linchpin of the regression safety net: for any seed, composing twice - with the ornamentation processor
///     shuffle on or off - must produce byte-identical MIDI. This forces every randomness leak in the composition
///     pipeline closed.
/// </summary>
[TestFixture]
internal sealed class EngineDeterminismTests
{
    private const int SampleIterations = 5;

    [Test]
    public void Compose_WithSameSeedAndShuffleDisabled_ProducesIdenticalMidi() => Gen.Int.Sample(
        seed =>
        {
            var configuration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };

            var first = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));
            var second = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

            first.Should().NotBeEmpty();
            first.Should().Equal(second);
        },
        iter: SampleIterations
    );

    [Test]
    public void Compose_WithSameSeedAndShuffleEnabled_ProducesIdenticalMidi() => Gen.Int.Sample(
        seed =>
        {
            // the processor shuffle draws from its own seed-derived stream, so the default (shuffle on) is reproducible too
            var configuration = TestCompositionConfigurations.Get(3, 10);

            var first = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));
            var second = SeededComposition.Notes(SeededComposition.Compose(configuration, seed));

            first.Should().NotBeEmpty();
            first.Should().Equal(second);
        },
        iter: SampleIterations
    );

    [Test]
    public void Compose_WithDifferentSeeds_ProducesDifferentMidi()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get(3, 10) with { ShuffleOrnamentationProcessors = false };

        // act
        var fromSeedOne = SeededComposition.Notes(SeededComposition.Compose(configuration, 1));
        var fromSeedTwo = SeededComposition.Notes(SeededComposition.Compose(configuration, 2));

        // assert
        fromSeedOne.Should().NotEqual(fromSeedTwo);
    }
}
