using BaroquenMelody.Library.Infrastructure.Collections.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Infrastructure.Collections.Extensions;

[TestFixture]
internal sealed class EnumerableExtensionsTests
{
    [Test]
    public void TrimEdges_trims_the_first_and_last_items()
    {
        // arrange
        var source = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var expected = new[] { 2, 3, 4, 5, 6, 7, 8, 9 };

        // act
        var trimmed = source.TrimEdges();

        // assert
        trimmed.Should().BeEquivalentTo(expected);
    }

    [Test]
    public void TrimEdges_returns_empty_list_when_count_is_larger_than_list()
    {
        // arrange
        var source = new[] { 1 };

        // act
        var trimmed = source.TrimEdges(int.MaxValue);

        // assert
        trimmed.Should().BeEmpty();
    }
}
