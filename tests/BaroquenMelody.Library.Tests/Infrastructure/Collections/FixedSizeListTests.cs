using BaroquenMelody.Library.Infrastructure.Collections;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Infrastructure.Collections;

[TestFixture]
internal sealed class FixedSizeListTests
{
    [Test]
    public void Constructor_WithSize_CreatesEmptyList()
    {
        var list = new FixedSizeList<int>(5);

        list.Count.Should().Be(0);
    }

    [Test]
    public void Constructor_WithSizeAndItems_CreatesListWithItems()
    {
        var initialItems = new[] { 1, 2, 3 };

        var list = new FixedSizeList<int>(5, initialItems);

        list.Count.Should().Be(3);
        list.Should().Equal(initialItems);
    }

    [Test]
    public void Add_WhenBelowCapacity_AddsItemToEnd()
    {
        var list = new FixedSizeList<int>(2)
        {
            1,
            2
        };

        list.Should().Equal([1, 2]);
    }

    [Test]
    public void Add_WhenAtCapacity_RemovesFirstItemAndAddsNewItemToEnd()
    {
        var list = new FixedSizeList<int>(2)
        {
            1,
            2,
            3
        };

        list.Should().Equal([2, 3]);
    }

    [Test]
    public void GetEnumerator_EnumeratesAllItems()
    {
        var list = new FixedSizeList<int>(3)
        {
            1,
            2,
            3
        };

        var items = list.ToList();

        items.Should().Equal([1, 2, 3]);
    }

    [Test]
    public void Count_ReturnsNumberOfItems()
    {
        var list = new FixedSizeList<int>(3)
        {
            1,
            2
        };

        list.Count.Should().Be(2);
    }

    [Test]
    public void Indexer_RetrievesCorrectItem()
    {
        var list = new FixedSizeList<int>(3)
        {
            1,
            2,
            3
        };

        list.Should().HaveElementAt(1, 2);
    }

    [Test]
    public void Indexer_WithInvalidIndex_ThrowsException()
    {
        var list = new FixedSizeList<int>(2) { 1 };

        var act = () => { var item = list[2]; };

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
