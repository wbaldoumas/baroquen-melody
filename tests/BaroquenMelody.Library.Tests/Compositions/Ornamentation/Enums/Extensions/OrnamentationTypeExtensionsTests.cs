using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Enums.Extensions;

[TestFixture]
internal sealed class OrnamentationTypeExtensionsTests
{
    [Test]
    [TestCase(OrnamentationType.DecorateInterval, 3)]
    [TestCase(OrnamentationType.DelayedRun, 4)]
    [TestCase(OrnamentationType.DoublePassingTone, 2)]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, 2)]
    [TestCase(OrnamentationType.DoubleRun, 7)]
    [TestCase(OrnamentationType.DoubleTurn, 7)]
    [TestCase(OrnamentationType.InvertedTurn, 3)]
    [TestCase(OrnamentationType.Mordent, 2)]
    [TestCase(OrnamentationType.NeighborTone, 1)]
    [TestCase(OrnamentationType.DelayedNeighborTone, 1)]
    [TestCase(OrnamentationType.PassingTone, 1)]
    [TestCase(OrnamentationType.DelayedPassingTone, 1)]
    [TestCase(OrnamentationType.Pedal, 3)]
    [TestCase(OrnamentationType.RepeatedNote, 1)]
    [TestCase(OrnamentationType.DelayedRepeatedNote, 1)]
    [TestCase(OrnamentationType.Run, 3)]
    [TestCase(OrnamentationType.Turn, 3)]
    [TestCase(OrnamentationType.Pickup, 1)]
    [TestCase(OrnamentationType.DelayedPickup, 1)]
    [TestCase(OrnamentationType.None, 0)]
    [TestCase(OrnamentationType.Sustain, 0)]
    [TestCase(OrnamentationType.MidSustain, 0)]
    [TestCase(OrnamentationType.Rest, 0)]
    public void OrnamentationCount_returns_expected_value(OrnamentationType ornamentationType, int expectedCount)
    {
        ornamentationType.OrnamentationCount().Should().Be(expectedCount);
    }

    [Test]
    public void OrnamentationCount_throws_when_ornamentation_type_not_found()
    {
        // act
        var action = () => ((OrnamentationType)byte.MaxValue).OrnamentationCount();

        // assert
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void OrnamentationCount_handles_all_ornamentation_types()
    {
        foreach (var ornamentationType in EnumUtils<OrnamentationType>.AsEnumerable())
        {
            // act
            var action = () => ornamentationType.OrnamentationCount();

            // assert
            action.Should().NotThrow();
        }
    }
}
