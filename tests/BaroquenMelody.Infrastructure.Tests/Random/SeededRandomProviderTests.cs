using BaroquenMelody.Infrastructure.Random;
using CsCheck;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Infrastructure.Tests.Random;

[TestFixture]
internal sealed class SeededRandomProviderTests
{
    private const int SequenceLength = 50;

    [Test]
    public void Next_WithSameSeed_ProducesIdenticalSequences() => Gen.Int.Sample(seed =>
    {
        // arrange
        var first = new SeededRandomProvider(seed);
        var second = new SeededRandomProvider(seed);

        // act / assert
        for (var i = 0; i < SequenceLength; i++)
        {
            first.Next().Should().Be(second.Next());
        }
    });

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(100)]
    public void Next_WithMaxValue_ReturnsValueWithinRange(int maxValue)
    {
        // arrange
        var provider = new SeededRandomProvider(maxValue);

        // act
        var result = provider.Next(maxValue);

        // assert
        result.Should().BeGreaterThanOrEqualTo(0).And.BeLessThan(maxValue);
    }

    [TestCase(10, 20)]
    [TestCase(100, 200)]
    [TestCase(-5, 5)]
    public void Next_WithMinAndMaxValue_ReturnsValueWithinRange(int minValue, int maxValue)
    {
        // arrange
        var provider = new SeededRandomProvider(minValue);

        // act
        var result = provider.Next(minValue, maxValue);

        // assert
        result.Should().BeGreaterThanOrEqualTo(minValue).And.BeLessThan(maxValue);
    }

    [Test]
    public void Next_ReturnsNonNegativeValue()
    {
        // arrange
        var provider = new SeededRandomProvider(42);

        // act
        var result = provider.Next();

        // assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }
}
