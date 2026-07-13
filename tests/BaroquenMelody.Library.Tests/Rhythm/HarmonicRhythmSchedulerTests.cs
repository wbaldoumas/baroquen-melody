using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rhythm;

[TestFixture]
internal sealed class HarmonicRhythmSchedulerTests
{
    [Test]
    [TestCaseSource(nameof(DefaultConfigurationTestCases))]
    public void ShouldHoldBeat_WithTheDefaultConfiguration_HoldsWeakBeatsOfPhraseInteriorMeasures(int measureIndex, int beatIndex, bool expectedShouldHold)
    {
        // arrange - the default phrasing configuration has a minimum phrase length of two measures, and a null
        // harmonic rhythm configuration falls back to the enabled default
        var harmonicRhythmScheduler = new HarmonicRhythmScheduler(TestCompositionConfigurations.Get());

        // act
        var shouldHold = harmonicRhythmScheduler.ShouldHoldBeat(measureIndex, beatIndex);

        // assert
        shouldHold.Should().Be(expectedShouldHold);
    }

    [Test]
    public void ShouldHoldBeat_WhenHarmonicRhythmIsDisabled_NeverHolds()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get() with
        {
            HarmonicRhythmConfiguration = new HarmonicRhythmConfiguration(Enabled: false)
        };

        var harmonicRhythmScheduler = new HarmonicRhythmScheduler(compositionConfiguration);

        // act & assert
        for (var measureIndex = 0; measureIndex < 4; measureIndex++)
        {
            for (var beatIndex = 0; beatIndex < 4; beatIndex++)
            {
                harmonicRhythmScheduler.ShouldHoldBeat(measureIndex, beatIndex).Should().BeFalse();
            }
        }
    }

    [Test]
    public void ShouldHoldBeat_WithALongerMinimumPhraseLength_AcceleratesOnlyBeforeThePhraseSeam()
    {
        // arrange - a minimum phrase length of four means measures 0-2 are phrase-interior and measure 3
        // accelerates into the seam. The phrasing configuration is constructed fresh rather than via a `with`
        // expression, since MinPhraseLength is computed in a property initializer that non-destructive
        // mutation does not re-run.
        var compositionConfiguration = TestCompositionConfigurations.Get() with
        {
            PhrasingConfiguration = new PhrasingConfiguration(PhraseLengths: [4, 8])
        };

        var harmonicRhythmScheduler = new HarmonicRhythmScheduler(compositionConfiguration);

        // act & assert
        harmonicRhythmScheduler.ShouldHoldBeat(0, 1).Should().BeTrue();
        harmonicRhythmScheduler.ShouldHoldBeat(1, 1).Should().BeTrue();
        harmonicRhythmScheduler.ShouldHoldBeat(2, 3).Should().BeTrue();
        harmonicRhythmScheduler.ShouldHoldBeat(3, 1).Should().BeFalse();
        harmonicRhythmScheduler.ShouldHoldBeat(3, 3).Should().BeFalse();
        harmonicRhythmScheduler.ShouldHoldBeat(4, 1).Should().BeTrue();
    }

    private static IEnumerable<TestCaseData> DefaultConfigurationTestCases()
    {
        yield return new TestCaseData(0, 0, false).SetName("The first beat of a phrase-interior measure composes fresh.");
        yield return new TestCaseData(0, 1, true).SetName("The second beat of a phrase-interior measure holds.");
        yield return new TestCaseData(0, 2, false).SetName("The third beat of a phrase-interior measure composes fresh.");
        yield return new TestCaseData(0, 3, true).SetName("The fourth beat of a phrase-interior measure holds.");
        yield return new TestCaseData(1, 0, false).SetName("The first beat of an acceleration measure composes fresh.");
        yield return new TestCaseData(1, 1, false).SetName("The second beat of an acceleration measure composes fresh.");
        yield return new TestCaseData(1, 3, false).SetName("The fourth beat of an acceleration measure composes fresh.");
        yield return new TestCaseData(2, 1, true).SetName("The pattern repeats every phrase cycle.");
    }
}
