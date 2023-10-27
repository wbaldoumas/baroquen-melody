using BaroquenMelody.Library.Random;
using FluentAssertions;
using NUnit.Framework;
using System.Collections;

namespace BaroquenMelody.Library.Tests.Random;

[TestFixture]
internal sealed class SegmentedRandomTrueIndexSelectorTests
{
    private const int BitArraySize = 100000;

    private SegmentedRandomTrueIndexSelector _segmentedRandomTrueIndexSelector = null!;

    [SetUp]
    public void SetUp() => _segmentedRandomTrueIndexSelector = new SegmentedRandomTrueIndexSelector();

    [Test]
    [TestCaseSource(nameof(ValidBitArrayTestCases))]
    public void GivenValidBitArray_ReturnsValidIndex(BitArray bitArray)
    {
        // iterative test to ensure that the random index is valid each time for a large number of iterations
        for (var i = 0; i < 1000; i++)
        {
            // act
            var index = _segmentedRandomTrueIndexSelector.SelectRandomTrueIndex(bitArray);

            // assert
            index.Should()
                .BeInRange(0, bitArray.Length)
                .And.Match(_ => bitArray[index]);

            bitArray[index].Should().BeTrue();
        }
    }

    [Test]
    [TestCaseSource(nameof(InvalidBitArrayTestCases))]
    public void GivenInvalidBitArray_ThrowsInvalidOperationException(BitArray bitArray)
    {
        for (var i = 0; i < 1000; i++)
        {
            // act
            var act = () => _segmentedRandomTrueIndexSelector.SelectRandomTrueIndex(bitArray);

            // assert
            act.Should().Throw<InvalidOperationException>();
        }
    }

    private static IEnumerable<TestCaseData> ValidBitArrayTestCases
    {
        get
        {
            yield return new TestCaseData(new BitArray(BitArraySize, true)).SetName("BitArray with all true values");

            var halfTrueBitArray = new BitArray(BitArraySize);

            for (var i = 0; i < BitArraySize; i++)
            {
                halfTrueBitArray[i] = i % 2 == 0;
            }

            yield return new TestCaseData(halfTrueBitArray).SetName("BitArray with alternating true and false values");
        }
    }

    private static IEnumerable<TestCaseData> InvalidBitArrayTestCases
    {
        get
        {
            yield return new TestCaseData(new BitArray(0, false)).SetName("BitArray with no values");
            yield return new TestCaseData(new BitArray(BitArraySize, false)).SetName("BitArray with all false values");
        }
    }
}
