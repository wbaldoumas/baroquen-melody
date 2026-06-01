using BaroquenMelody.Library.Dynamics;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Dynamics;

[TestFixture]
internal sealed class BeatStrengthCalculatorTests
{
    [Test]

    // four-beat (hyper)measure: strong - weak - medium - weak.
    [TestCase(0, 4, MetricStrength.Strong)]
    [TestCase(1, 4, MetricStrength.Weak)]
    [TestCase(2, 4, MetricStrength.Medium)]
    [TestCase(3, 4, MetricStrength.Weak)]

    // the downbeat is always strong, regardless of measure length.
    [TestCase(0, 1, MetricStrength.Strong)]
    [TestCase(0, 2, MetricStrength.Strong)]
    [TestCase(0, 3, MetricStrength.Strong)]

    // short measures have no secondary accent.
    [TestCase(1, 2, MetricStrength.Weak)]
    [TestCase(1, 3, MetricStrength.Weak)]
    [TestCase(2, 3, MetricStrength.Weak)]

    // a six-beat measure places the secondary accent at its mid-point.
    [TestCase(3, 6, MetricStrength.Medium)]
    [TestCase(1, 6, MetricStrength.Weak)]
    public void Calculate_ReturnsExpectedStrength(int beatIndexInMeasure, int beatsPerMeasure, MetricStrength expected)
    {
        var result = BeatStrengthCalculator.Calculate(beatIndexInMeasure, beatsPerMeasure);

        result.Should().Be(expected);
    }
}
