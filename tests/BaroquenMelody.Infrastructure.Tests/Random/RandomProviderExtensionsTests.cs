using BaroquenMelody.Infrastructure.Random;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Infrastructure.Tests.Random;

[TestFixture]
internal sealed class RandomProviderExtensionsTests
{
    [Test]
    public void MinByRandom_SelectsElementWithSmallestRandomKey()
    {
        // arrange
        var source = new[] { "a", "b", "c" };
        var randomProvider = new QueuedRandomProvider(3, 1, 2);

        // act
        var result = source.MinByRandom(randomProvider);

        // assert
        result.Should().Be("b");
    }

    [Test]
    public void MinByRandom_WithEmptySource_ReturnsNull()
    {
        // arrange
        string[] source = [];
        var randomProvider = new QueuedRandomProvider(1);

        // act
        var result = source.MinByRandom(randomProvider);

        // assert
        result.Should().BeNull();
    }

    [Test]
    public void OrderByRandom_OrdersElementsByRandomKey()
    {
        // arrange
        var source = new[] { "a", "b", "c" };
        var randomProvider = new QueuedRandomProvider(3, 1, 2);

        // act
        var result = source.OrderByRandom(randomProvider).ToList();

        // assert
        result.Should().Equal("b", "c", "a");
    }

    [Test]
    public void OrderByRandom_PreservesAllElements()
    {
        // arrange
        var source = new[] { "a", "b", "c", "d" };
        var randomProvider = new QueuedRandomProvider(4, 3, 2, 1);

        // act
        var result = source.OrderByRandom(randomProvider).ToList();

        // assert
        result.Should().BeEquivalentTo(source);
    }
}
