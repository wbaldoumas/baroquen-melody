using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
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
    public void BeatsPerMeasure_throws_on_unsupported_meter()
    {
        var act = () => ((Meter)int.MaxValue).BeatsPerMeasure();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    [TestCaseSource(nameof(DefaultMusicalTimeSpanTestCases))]
    public void DefaultMusicalTimeSpan_returns_expected_value(Meter meter, MusicalTimeSpan expectedDefaultMusicalTimeSpan) =>
        meter.DefaultMusicalTimeSpan().Should().Be(expectedDefaultMusicalTimeSpan);

    [Test]
    public void DefaultMusicalTimeSpan_throws_on_unsupported_meter()
    {
        var act = () => ((Meter)int.MaxValue).DefaultMusicalTimeSpan();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    [TestCase(Meter.FourFour, "4/4")]
    [TestCase(Meter.ThreeFour, "3/4")]
    public void AsString_generates_expected_string(Meter meter, string expected)
    {
        meter.AsString().Should().Be(expected);
    }

    [Test]
    public void AsString_throws_on_unsupported_meter()
    {
        var act = () => ((Meter)int.MaxValue).AsString();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    private static IEnumerable<TestCaseData> DefaultMusicalTimeSpanTestCases
    {
        get
        {
            yield return new TestCaseData(Meter.FourFour, MusicalTimeSpan.Half);
            yield return new TestCaseData(Meter.ThreeFour, MusicalTimeSpan.Half.Dotted(1));
        }
    }
}
