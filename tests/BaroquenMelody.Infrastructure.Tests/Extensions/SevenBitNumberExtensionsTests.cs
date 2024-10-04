using BaroquenMelody.Infrastructure.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.Common;
using NUnit.Framework;

namespace BaroquenMelody.Infrastructure.Tests.Extensions;

[TestFixture]
internal sealed class SevenBitNumberExtensionsTests
{
    [Test]
    public void Increment_increments_the_given_seven_bit_number_by_1()
    {
        // arrange
        var source = new SevenBitNumber(99);
        var expected = new SevenBitNumber(100);

        // act
        var result = source.Increment();

        // assert
        result.Should().Be(expected);
    }

    [Test]
    public void Decrement_decrements_the_given_seven_bit_number_by_1()
    {
        // arrange
        var source = new SevenBitNumber(99);
        var expected = new SevenBitNumber(98);

        // act
        var result = source.Decrement();

        // assert
        result.Should().Be(expected);
    }
}
