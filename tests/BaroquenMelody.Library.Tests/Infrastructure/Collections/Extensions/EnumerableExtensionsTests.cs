using BaroquenMelody.Library.Infrastructure.Collections.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Infrastructure.Collections.Extensions;

[TestFixture]
internal sealed class EnumerableExtensionsTests
{
    [Test]
    public void TrimEdges_trims_expected_values()
    {
        // arrange
        var source = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var expected = new[] { 2, 3, 4, 5, 6, 7, 8, 9 };

        // act
        var result = source.TrimEdges();

        // assert
        result.Should().BeEquivalentTo(expected);
    }
}
