using BaroquenMelody.Infrastructure.Random;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Infrastructure.Tests.Random;

[TestFixture]
internal sealed class ThreadLocalRandomProviderTests
{
    private ThreadLocalRandomProvider _provider = null!;

    [SetUp]
    public void SetUp() => _provider = new ThreadLocalRandomProvider();

    [TestCase(1)]
    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    public void Next_WithMaxValue_ReturnsValueWithinRange(int maxValue)
    {
        // act
        var result = _provider.Next(maxValue);

        // assert
        result.Should().BeGreaterThanOrEqualTo(0).And.BeLessThan(maxValue);
    }

    [TestCase(10, 20)]
    [TestCase(100, 200)]
    [TestCase(1000, 2000)]
    public void Next_WithMinAndMaxValue_ReturnsValueWithinRange(int minValue, int maxValue)
    {
        // act
        var result = _provider.Next(minValue, maxValue);

        // assert
        result.Should().BeGreaterThanOrEqualTo(minValue).And.BeLessThan(maxValue);
    }

    [Test]
    public void Next_ReturnsNonNegativeValue()
    {
        // act
        var result = _provider.Next();

        // assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }
}
