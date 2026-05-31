using BaroquenMelody.Infrastructure.Random;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Infrastructure.Tests.Random;

[TestFixture]
internal sealed class WeightedRandomBooleanGeneratorTests
{
    [TestCase(50, 30, true)]
    [TestCase(31, 30, true)]
    [TestCase(30, 30, false)]
    [TestCase(20, 30, false)]
    [TestCase(100, 99, true)]
    [TestCase(0, 0, false)]
    public void IsTrue_ReturnsWhetherWeightExceedsTheProvidedRandomValue(int weight, int randomValue, bool expected)
    {
        // arrange
        var randomProvider = new QueuedRandomProvider(randomValue);
        var generator = new WeightedRandomBooleanGenerator(randomProvider);

        // act
        var result = generator.IsTrue(weight);

        // assert
        result.Should().Be(expected);
    }

    [Test]
    public void IsTrue_WithDefaultWeight_UsesAWeightOfFifty()
    {
        // arrange
        var randomProvider = new QueuedRandomProvider(49);
        var generator = new WeightedRandomBooleanGenerator(randomProvider);

        // act
        var result = generator.IsTrue();

        // assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsTrue_WithDefaultProvider_ReturnsTrueForFullWeight()
    {
        // arrange
        var generator = new WeightedRandomBooleanGenerator();

        // act
        var result = generator.IsTrue(100);

        // assert
        result.Should().BeTrue();
    }

    [Test]
    public void IsTrue_WithDefaultProvider_ReturnsFalseForZeroWeight()
    {
        // arrange
        var generator = new WeightedRandomBooleanGenerator();

        // act
        var result = generator.IsTrue(0);

        // assert
        result.Should().BeFalse();
    }
}
