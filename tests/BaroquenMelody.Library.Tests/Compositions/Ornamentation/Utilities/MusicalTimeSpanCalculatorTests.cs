﻿using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Utilities;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Utilities;

[TestFixture]
internal sealed class MusicalTimeSpanCalculatorTests
{
    private MusicalTimeSpanCalculator _musicalTimeSpanCalculator = null!;

    [SetUp]
    public void SetUp() => _musicalTimeSpanCalculator = new MusicalTimeSpanCalculator();

    [Test]
    [TestCaseSource(nameof(PrimaryNoteTestCases))]
    public void CalculatePrimaryNoteTimeSpan_ShouldReturnExpectedTimeSpan(
        OrnamentationType ornamentationType,
        Meter meter,
        MusicalTimeSpan expectedPrimaryNoteMusicalTimeSpan)
    {
        // act
        var primaryNoteMusicalTimeSpan = _musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, meter);

        // assert
        primaryNoteMusicalTimeSpan.Should().Be(expectedPrimaryNoteMusicalTimeSpan);
    }

    [Test]
    [TestCaseSource(nameof(OrnamentedNoteTestCases))]
    public void CalculateOrnamentationTimeSpan_ShouldReturnExpectedTimeSpan(
        OrnamentationType ornamentationType,
        Meter meter,
        MusicalTimeSpan expectedOrnamentationMusicalTimeSpan,
        int ornamentationStep)
    {
        // act
        var ornamentationMusicalTimeSpan = _musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, meter, ornamentationStep);

        // assert
        ornamentationMusicalTimeSpan.Should().Be(expectedOrnamentationMusicalTimeSpan);
    }

    [Test]
    [TestCase(OrnamentationType.PassingTone, (Meter)9001)]
    [TestCase((OrnamentationType)250, Meter.FourFour)]
    public void CalculatePrimaryNoteTimeSpan_WhenOrnamentationOrMeterIsOutOfRange_ThenArgumentOutOfRangeExceptionIsThrown(OrnamentationType ornamentationType, Meter meter)
    {
        // act
        var act = () => _musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, meter);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    [TestCase(OrnamentationType.PassingTone, (Meter)9001)]
    [TestCase((OrnamentationType)250, Meter.FourFour)]
    public void CalculateOrnamentationTimeSpan_WhenOrnamentationOrMeterIsOutOfRange_ThenArgumentOutOfRangeExceptionIsThrown(OrnamentationType ornamentationType, Meter meter)
    {
        // act
        var act = () => _musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, meter);

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    [TestCase(OrnamentationType.None)]
    [TestCase(OrnamentationType.Sustain)]
    public void CalculateOrnamentationTimeSpan_WhenOrnamentationTypeIsNotSupported_ThenNotSupportedExceptionIsThrown(OrnamentationType ornamentationType)
    {
        // act
        var act = () => _musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, Meter.FourFour);

        // assert
        act.Should().Throw<NotSupportedException>();
    }

    private static IEnumerable<TestCaseData> PrimaryNoteTestCases
    {
        get
        {
            yield return new TestCaseData(OrnamentationType.None, Meter.FourFour, MusicalTimeSpan.Quarter);

            yield return new TestCaseData(OrnamentationType.PassingTone, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.SixteenthNoteRun, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.DelayedPassingTone, Meter.FourFour, MusicalTimeSpan.Eighth.Dotted(1));

            yield return new TestCaseData(OrnamentationType.Turn, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.AlternateTurn, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.Sustain, Meter.FourFour, MusicalTimeSpan.Half);

            yield return new TestCaseData(OrnamentationType.MidSustain, Meter.FourFour, new MusicalTimeSpan());

            yield return new TestCaseData(OrnamentationType.DoubleTurn, Meter.FourFour, MusicalTimeSpan.ThirtySecond);

            yield return new TestCaseData(OrnamentationType.DelayedThirtySecondNoteRun, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.DoublePassingTone, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.DelayedDoublePassingTone, Meter.FourFour, MusicalTimeSpan.Eighth.Dotted(1));

            yield return new TestCaseData(OrnamentationType.DecorateInterval, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.ThirtySecondNoteRun, Meter.FourFour, MusicalTimeSpan.ThirtySecond);

            yield return new TestCaseData(OrnamentationType.Pedal, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.Mordent, Meter.FourFour, MusicalTimeSpan.ThirtySecond);

            yield return new TestCaseData(OrnamentationType.Rest, Meter.FourFour, new MusicalTimeSpan());

            yield return new TestCaseData(OrnamentationType.RepeatedEighthNote, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.RepeatedDottedEighthSixteenth, Meter.FourFour, MusicalTimeSpan.Eighth.Dotted(1));

            yield return new TestCaseData(OrnamentationType.NeighborTone, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.DelayedNeighborTone, Meter.FourFour, MusicalTimeSpan.Eighth.Dotted(1));
        }
    }

    private static IEnumerable<TestCaseData> OrnamentedNoteTestCases
    {
        get
        {
            yield return new TestCaseData(OrnamentationType.PassingTone, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.SixteenthNoteRun, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedPassingTone, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.Turn, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.AlternateTurn, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.MidSustain, Meter.FourFour, new MusicalTimeSpan(), 1);

            yield return new TestCaseData(OrnamentationType.DoubleTurn, Meter.FourFour, MusicalTimeSpan.ThirtySecond, 1);

            yield return new TestCaseData(OrnamentationType.DelayedThirtySecondNoteRun, Meter.FourFour, MusicalTimeSpan.ThirtySecond, 1);

            yield return new TestCaseData(OrnamentationType.DoublePassingTone, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedDoublePassingTone, Meter.FourFour, MusicalTimeSpan.ThirtySecond, 1);

            yield return new TestCaseData(OrnamentationType.DecorateInterval, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.ThirtySecondNoteRun, Meter.FourFour, MusicalTimeSpan.ThirtySecond, 1);

            yield return new TestCaseData(OrnamentationType.Pedal, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.Mordent, Meter.FourFour, MusicalTimeSpan.ThirtySecond, 1);

            yield return new TestCaseData(OrnamentationType.Mordent, Meter.FourFour, MusicalTimeSpan.Eighth.Dotted(1), 2);

            yield return new TestCaseData(OrnamentationType.Rest, Meter.FourFour, new MusicalTimeSpan(), 1);

            yield return new TestCaseData(OrnamentationType.RepeatedEighthNote, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.RepeatedDottedEighthSixteenth, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.NeighborTone, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedNeighborTone, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);
        }
    }
}
