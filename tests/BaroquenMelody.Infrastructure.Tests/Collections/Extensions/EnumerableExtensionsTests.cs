using BaroquenMelody.Infrastructure.Collections.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Infrastructure.Tests.Collections.Extensions;

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

    [Test]
    public void ToPowerSet_generates_all_expected_subsets()
    {
        // arrange
        var source = new HashSet<NoteName> { NoteName.G, NoteName.B, NoteName.D, NoteName.F };

        var expected = new List<HashSet<NoteName>>
        {
            new() { NoteName.G, NoteName.B, NoteName.D, NoteName.F },
            new() { NoteName.G, NoteName.B, NoteName.D },
            new() { NoteName.G, NoteName.B, NoteName.F },
            new() { NoteName.G, NoteName.D, NoteName.F },
            new() { NoteName.B, NoteName.D, NoteName.F },
            new() { NoteName.G, NoteName.B },
            new() { NoteName.G, NoteName.D },
            new() { NoteName.G, NoteName.F },
            new() { NoteName.B, NoteName.D },
            new() { NoteName.B, NoteName.F },
            new() { NoteName.D, NoteName.F },
            new() { NoteName.G },
            new() { NoteName.B },
            new() { NoteName.D },
            new() { NoteName.F },
            new()
        };

        // act
        var powerSet = source.ToPowerSet().ToList();

        // assert
        powerSet.Should().HaveSameCount(expected);

        foreach (var subset in powerSet)
        {
            expected.Should().ContainEquivalentOf(subset);
        }
    }

    [Test]
    public void GetContentHashCode_generates_same_hash_code_irrespective_of_order()
    {
        // arrange
        var sourceA = new HashSet<NoteName> { NoteName.G, NoteName.B, NoteName.D, NoteName.F };
        var sourceB = new HashSet<NoteName> { NoteName.B, NoteName.D, NoteName.F, NoteName.G };
        var sourceC = new HashSet<NoteName> { NoteName.D, NoteName.F, NoteName.G, NoteName.B };

        var otherSource = new HashSet<NoteName> { NoteName.G, NoteName.C, NoteName.A, NoteName.F };

        // act
        var hashCodeA = sourceA.GetContentHashCode();
        var hashCodeB = sourceB.GetContentHashCode();
        var hashCodeC = sourceC.GetContentHashCode();
        var otherHashCode = otherSource.GetContentHashCode();

        // assert
        hashCodeA.Should().NotBe(0)
            .And.Be(hashCodeB)
            .And.Be(hashCodeC)
            .And.NotBe(otherHashCode);
    }
}
