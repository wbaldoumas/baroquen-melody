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
    [TestCaseSource(nameof(TestCases))]
    public void CalculatePrimaryNoteTimeSpan_ShouldReturnExpectedTimeSpan(
        OrnamentationType ornamentationType,
        Meter meter,
        MusicalTimeSpan expectedPrimaryNoteMusicalTimeSpan,
        MusicalTimeSpan expectedOrnamentationMusicalTimeSpan)
    {
        // act
        var primaryNoteMusicalTimeSpan = _musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, meter);
        var ornamentationMusicalTimeSpan = _musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, meter);

        // assert
        primaryNoteMusicalTimeSpan.Should().Be(expectedPrimaryNoteMusicalTimeSpan);
        ornamentationMusicalTimeSpan.Should().Be(expectedOrnamentationMusicalTimeSpan);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(OrnamentationType.PassingTone, Meter.FourFour, MusicalTimeSpan.Eighth, MusicalTimeSpan.Eighth);

            // more test cases to come as more ornamentation types and meters are added...
        }
    }

    [Test]
    [TestCase(OrnamentationType.PassingTone, (Meter)9001)]
    [TestCase((OrnamentationType)9001, Meter.FourFour)]
    public void WhenOrnamentationOrMeterIsOutOfRange_ThenArgumentOutOfRangeExceptionIsThrown(OrnamentationType ornamentationType, Meter meter)
    {
        // act
        var actPrimary = () => _musicalTimeSpanCalculator.CalculatePrimaryNoteTimeSpan(ornamentationType, meter);
        var actOrnamentation = () => _musicalTimeSpanCalculator.CalculateOrnamentationTimeSpan(ornamentationType, meter);

        // assert
        actPrimary.Should().Throw<ArgumentOutOfRangeException>();
        actOrnamentation.Should().Throw<ArgumentOutOfRangeException>();
    }
}
