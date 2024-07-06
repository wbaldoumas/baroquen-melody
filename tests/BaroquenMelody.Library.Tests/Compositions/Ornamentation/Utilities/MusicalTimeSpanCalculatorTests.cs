using BaroquenMelody.Library.Compositions.Enums;
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
        MusicalTimeSpan expectedOrnamentationMusicalTimeSpan)
    {
        // act
        var ornamentationMusicalTimeSpan = _musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, meter);

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

            yield return new TestCaseData(OrnamentationType.Rest, Meter.FourFour, new MusicalTimeSpan());

            yield return new TestCaseData(OrnamentationType.DoubleTurn, Meter.FourFour, MusicalTimeSpan.ThirtySecond);

            yield return new TestCaseData(OrnamentationType.DelayedThirtySecondNoteRun, Meter.FourFour, MusicalTimeSpan.Eighth);

            // more test cases to come as more ornamentation types and meters are added...
        }
    }

    private static IEnumerable<TestCaseData> OrnamentedNoteTestCases
    {
        get
        {
            yield return new TestCaseData(OrnamentationType.PassingTone, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.SixteenthNoteRun, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.DelayedPassingTone, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.Turn, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.AlternateTurn, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.Rest, Meter.FourFour, new MusicalTimeSpan());

            yield return new TestCaseData(OrnamentationType.DoubleTurn, Meter.FourFour, MusicalTimeSpan.ThirtySecond);

            yield return new TestCaseData(OrnamentationType.DelayedThirtySecondNoteRun, Meter.FourFour, MusicalTimeSpan.ThirtySecond);

            // more test cases to come as more ornamentation types and meters are added...
        }
    }
}
