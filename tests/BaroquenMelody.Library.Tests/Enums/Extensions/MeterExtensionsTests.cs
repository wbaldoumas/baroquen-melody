﻿using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Enums.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Enums.Extensions;

[TestFixture]
internal sealed class MeterExtensionsTests
{
    [Test]
    [TestCase(Meter.FourFour, 4)]
    [TestCase(Meter.ThreeFour, 4)]
    [TestCase(Meter.FiveEight, 4)]
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
    [TestCase(Meter.FiveEight, "5/8")]
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
            yield return new TestCaseData(Meter.FiveEight, MusicalTimeSpan.Half + MusicalTimeSpan.Eighth);
        }
    }

    [Test]
    [TestCase(Meter.FourFour, 4, 4)]
    [TestCase(Meter.ThreeFour, 3, 4)]
    [TestCase(Meter.FiveEight, 5, 8)]
    public void ToTimeSignature_returns_expected_value(Meter meter, int expectedNumerator, int expectedDenominator)
    {
        // arrange
        var expected = new TimeSignature(expectedNumerator, expectedDenominator);

        // act
        var actual = meter.ToTimeSignature();

        // assert
        actual.Should().Be(expected);
    }

    [Test]
    public void ToTimeSignature_throws_on_unsupported_meter()
    {
        var act = () => ((Meter)int.MaxValue).ToTimeSignature();

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
