using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Enums.Extensions;
using BaroquenMelody.Library.Ornamentation.Utilities;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Utilities;

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

    [Test]
    public void MusicalTimeSpanCalculator_handles_all_ornamentation_types()
    {
        foreach (var ornamentationType in EnumUtils<OrnamentationType>.AsEnumerable())
        {
            var act = () => _musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, Meter.FourFour);

            act.Should().NotThrow();
        }

        foreach (var ornamentationType in EnumUtils<OrnamentationType>.AsEnumerable().Where(ornamentationType => ornamentationType != OrnamentationType.None))
        {
            var ornamentationCount = ornamentationType.OrnamentationCount();

            for (var i = 0; i < ornamentationCount; i++)
            {
                var ornamentationStep = i;

                var act = () => _musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, Meter.FourFour, ornamentationStep);

                act.Should().NotThrow();
            }
        }
    }

    private static IEnumerable<TestCaseData> PrimaryNoteTestCases
    {
        get
        {
            yield return new TestCaseData(OrnamentationType.None, Meter.FourFour, MusicalTimeSpan.Half);

            yield return new TestCaseData(OrnamentationType.None, Meter.ThreeFour, MusicalTimeSpan.Half.Dotted(1));

            yield return new TestCaseData(OrnamentationType.PassingTone, Meter.FourFour, MusicalTimeSpan.Quarter);

            yield return new TestCaseData(OrnamentationType.PassingTone, Meter.ThreeFour, MusicalTimeSpan.Half);

            yield return new TestCaseData(OrnamentationType.Run, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.Run, Meter.ThreeFour, MusicalTimeSpan.Quarter.Dotted(1));

            yield return new TestCaseData(OrnamentationType.DelayedPassingTone, Meter.FourFour, MusicalTimeSpan.Quarter.Dotted(1));

            yield return new TestCaseData(OrnamentationType.DelayedPassingTone, Meter.ThreeFour, MusicalTimeSpan.Half + MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.Turn, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.Turn, Meter.ThreeFour, MusicalTimeSpan.Quarter.Dotted(1));

            yield return new TestCaseData(OrnamentationType.InvertedTurn, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.InvertedTurn, Meter.ThreeFour, MusicalTimeSpan.Quarter.Dotted(1));

            yield return new TestCaseData(OrnamentationType.Sustain, Meter.FourFour, MusicalTimeSpan.Whole);

            yield return new TestCaseData(OrnamentationType.Sustain, Meter.ThreeFour, MusicalTimeSpan.Half.Dotted(1) + MusicalTimeSpan.Half.Dotted(1));

            yield return new TestCaseData(OrnamentationType.MidSustain, Meter.FourFour, new MusicalTimeSpan());

            yield return new TestCaseData(OrnamentationType.MidSustain, Meter.ThreeFour, new MusicalTimeSpan());

            yield return new TestCaseData(OrnamentationType.DoubleInvertedTurn, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.DoubleInvertedTurn, Meter.ThreeFour, MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.DelayedRun, Meter.FourFour, MusicalTimeSpan.Quarter);

            yield return new TestCaseData(OrnamentationType.DelayedRun, Meter.ThreeFour, MusicalTimeSpan.Quarter);

            yield return new TestCaseData(OrnamentationType.DoublePassingTone, Meter.FourFour, MusicalTimeSpan.Quarter);

            yield return new TestCaseData(OrnamentationType.DoublePassingTone, Meter.ThreeFour, MusicalTimeSpan.Quarter);

            yield return new TestCaseData(OrnamentationType.DelayedDoublePassingTone, Meter.FourFour, MusicalTimeSpan.Quarter.Dotted(1));

            yield return new TestCaseData(OrnamentationType.DelayedDoublePassingTone, Meter.ThreeFour, MusicalTimeSpan.Half);

            yield return new TestCaseData(OrnamentationType.DecorateInterval, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.DecorateInterval, Meter.ThreeFour, MusicalTimeSpan.Quarter.Dotted(1));

            yield return new TestCaseData(OrnamentationType.DoubleRun, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.DoubleRun, Meter.ThreeFour, MusicalTimeSpan.Quarter + MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.Pedal, Meter.FourFour, MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.Pedal, Meter.ThreeFour, MusicalTimeSpan.Quarter.Dotted(1));

            yield return new TestCaseData(OrnamentationType.Mordent, Meter.FourFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.Mordent, Meter.ThreeFour, MusicalTimeSpan.Sixteenth);

            yield return new TestCaseData(OrnamentationType.Rest, Meter.FourFour, new MusicalTimeSpan());

            yield return new TestCaseData(OrnamentationType.Rest, Meter.ThreeFour, new MusicalTimeSpan());

            yield return new TestCaseData(OrnamentationType.RepeatedNote, Meter.FourFour, MusicalTimeSpan.Quarter);

            yield return new TestCaseData(OrnamentationType.RepeatedNote, Meter.ThreeFour, MusicalTimeSpan.Half);

            yield return new TestCaseData(OrnamentationType.DelayedRepeatedNote, Meter.FourFour, MusicalTimeSpan.Quarter.Dotted(1));

            yield return new TestCaseData(OrnamentationType.DelayedRepeatedNote, Meter.ThreeFour, MusicalTimeSpan.Half + MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.NeighborTone, Meter.FourFour, MusicalTimeSpan.Quarter);

            yield return new TestCaseData(OrnamentationType.NeighborTone, Meter.ThreeFour, MusicalTimeSpan.Half);

            yield return new TestCaseData(OrnamentationType.DelayedNeighborTone, Meter.FourFour, MusicalTimeSpan.Quarter.Dotted(1));

            yield return new TestCaseData(OrnamentationType.DelayedNeighborTone, Meter.ThreeFour, MusicalTimeSpan.Half + MusicalTimeSpan.Eighth);

            yield return new TestCaseData(OrnamentationType.Pickup, Meter.FourFour, MusicalTimeSpan.Quarter);

            yield return new TestCaseData(OrnamentationType.Pickup, Meter.ThreeFour, MusicalTimeSpan.Half);

            yield return new TestCaseData(OrnamentationType.DelayedPickup, Meter.FourFour, MusicalTimeSpan.Quarter.Dotted(1));

            yield return new TestCaseData(OrnamentationType.DelayedPickup, Meter.ThreeFour, MusicalTimeSpan.Half + MusicalTimeSpan.Eighth);
        }
    }

    private static IEnumerable<TestCaseData> OrnamentedNoteTestCases
    {
        get
        {
            yield return new TestCaseData(OrnamentationType.PassingTone, Meter.FourFour, MusicalTimeSpan.Quarter, 1);

            yield return new TestCaseData(OrnamentationType.PassingTone, Meter.ThreeFour, MusicalTimeSpan.Quarter, 1);

            yield return new TestCaseData(OrnamentationType.Run, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.Run, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedPassingTone, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedPassingTone, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.Turn, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.Turn, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.InvertedTurn, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.InvertedTurn, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.MidSustain, Meter.FourFour, new MusicalTimeSpan(), 1);

            yield return new TestCaseData(OrnamentationType.MidSustain, Meter.ThreeFour, new MusicalTimeSpan(), 1);

            yield return new TestCaseData(OrnamentationType.DoubleInvertedTurn, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.DoubleInvertedTurn, Meter.ThreeFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedRun, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedRun, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DoublePassingTone, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DoublePassingTone, Meter.ThreeFour, MusicalTimeSpan.Quarter, 1);

            yield return new TestCaseData(OrnamentationType.DelayedDoublePassingTone, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedDoublePassingTone, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DecorateInterval, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DecorateInterval, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DoubleRun, Meter.FourFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.DoubleRun, Meter.ThreeFour, MusicalTimeSpan.Sixteenth, 1);

            yield return new TestCaseData(OrnamentationType.Pedal, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.Pedal, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.Mordent, Meter.FourFour, MusicalTimeSpan.Sixteenth, 0);

            yield return new TestCaseData(OrnamentationType.Mordent, Meter.ThreeFour, MusicalTimeSpan.Sixteenth, 0);

            yield return new TestCaseData(OrnamentationType.Mordent, Meter.FourFour, MusicalTimeSpan.Quarter.Dotted(1), 1);

            yield return new TestCaseData(OrnamentationType.Mordent, Meter.ThreeFour, MusicalTimeSpan.Eighth + MusicalTimeSpan.Half, 1);

            yield return new TestCaseData(OrnamentationType.Rest, Meter.FourFour, new MusicalTimeSpan(), 1);

            yield return new TestCaseData(OrnamentationType.Rest, Meter.ThreeFour, new MusicalTimeSpan(), 1);

            yield return new TestCaseData(OrnamentationType.RepeatedNote, Meter.FourFour, MusicalTimeSpan.Quarter, 1);

            yield return new TestCaseData(OrnamentationType.RepeatedNote, Meter.ThreeFour, MusicalTimeSpan.Quarter, 1);

            yield return new TestCaseData(OrnamentationType.DelayedRepeatedNote, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedRepeatedNote, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.NeighborTone, Meter.FourFour, MusicalTimeSpan.Quarter, 1);

            yield return new TestCaseData(OrnamentationType.NeighborTone, Meter.ThreeFour, MusicalTimeSpan.Quarter, 1);

            yield return new TestCaseData(OrnamentationType.DelayedNeighborTone, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedNeighborTone, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.Pickup, Meter.FourFour, MusicalTimeSpan.Quarter, 1);

            yield return new TestCaseData(OrnamentationType.Pickup, Meter.ThreeFour, MusicalTimeSpan.Quarter, 1);

            yield return new TestCaseData(OrnamentationType.DelayedPickup, Meter.FourFour, MusicalTimeSpan.Eighth, 1);

            yield return new TestCaseData(OrnamentationType.DelayedPickup, Meter.ThreeFour, MusicalTimeSpan.Eighth, 1);
        }
    }
}
