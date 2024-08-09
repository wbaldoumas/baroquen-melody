using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Enums.Extensions;

[TestFixture]
internal sealed class MeterExtensionsTests
{
    [Test]
    [TestCase(Meter.FourFour, 4)]
    [TestCase(Meter.ThreeFour, 3)]
    public void BeatsPerMeasure_returns_expected_value(Meter meter, int expectedBeatsPerMeasure) =>
        meter.BeatsPerMeasure().Should().Be(expectedBeatsPerMeasure);

    [Test]
    public void BeatsPerMeasure_throws_on_unsupported_meter() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ((Meter)int.MaxValue).BeatsPerMeasure());
}
