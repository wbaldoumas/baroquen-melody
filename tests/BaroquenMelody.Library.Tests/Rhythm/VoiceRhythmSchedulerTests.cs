using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rhythm;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Rhythm;

[TestFixture]
internal sealed class VoiceRhythmSchedulerTests
{
    [Test]
    [TestCaseSource(nameof(HeldRotationTestCases))]
    public void TryGetHeldInstrument_WithTheDefaultConfiguration_RotatesTheHeldVoicePerBlockAndSkipsSeams(int measureIndex, bool expectedHasHeld, Instrument expectedHeldInstrument)
    {
        // arrange - a null voice rhythm configuration falls back to the enabled default, the default phrasing
        // configuration has a minimum phrase length of two measures, and four voices order One..Four by register
        var voiceRhythmScheduler = new VoiceRhythmScheduler(TestCompositionConfigurations.Get());

        // act
        var hasHeld = voiceRhythmScheduler.TryGetHeldInstrument(measureIndex, out var heldInstrument);

        // assert
        hasHeld.Should().Be(expectedHasHeld);

        if (expectedHasHeld)
        {
            heldInstrument.Should().Be(expectedHeldInstrument);
        }
    }

    [Test]
    [TestCaseSource(nameof(PinnedBeatTestCases))]
    public void TryGetPinnedInstrument_WithTheDefaultConfiguration_PinsOnlyTheFreshInteriorBeatsOfHeldMeasures(int measureIndex, int beatIndex, bool expectedHasPin, Instrument expectedPinnedInstrument)
    {
        // arrange
        var voiceRhythmScheduler = new VoiceRhythmScheduler(TestCompositionConfigurations.Get());

        // act
        var hasPin = voiceRhythmScheduler.TryGetPinnedInstrument(measureIndex, beatIndex, out var pinnedInstrument);

        // assert
        hasPin.Should().Be(expectedHasPin);

        if (expectedHasPin)
        {
            pinnedInstrument.Should().Be(expectedPinnedInstrument);
        }
    }

    [Test]
    [TestCaseSource(nameof(FloridRotationTestCases))]
    public void TryGetFloridInstrument_WithTheDefaultConfiguration_RotatesOneVoiceAheadOfTheHeldVoice(int measureIndex, Instrument expectedFloridInstrument)
    {
        // arrange
        var voiceRhythmScheduler = new VoiceRhythmScheduler(TestCompositionConfigurations.Get());

        // act
        var hasFlorid = voiceRhythmScheduler.TryGetFloridInstrument(measureIndex, out var floridInstrument);

        // assert
        hasFlorid.Should().BeTrue();
        floridInstrument.Should().Be(expectedFloridInstrument);
    }

    [Test]
    public void HeldAndFloridInstruments_AreAlwaysDistinct()
    {
        // arrange
        var voiceRhythmScheduler = new VoiceRhythmScheduler(TestCompositionConfigurations.Get(numberOfInstruments: 3));

        // act & assert
        for (var measureIndex = 0; measureIndex < 24; measureIndex++)
        {
            if (!voiceRhythmScheduler.TryGetHeldInstrument(measureIndex, out var heldInstrument))
            {
                continue;
            }

            voiceRhythmScheduler.TryGetFloridInstrument(measureIndex, out var floridInstrument).Should().BeTrue();
            floridInstrument.Should().NotBe(heldInstrument, "one voice cannot be both held and florid in the same block");
        }
    }

    [Test]
    public void TryGetHeldInstrument_WhenVoiceRhythmIsDisabled_AnswersNothing()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get() with
        {
            VoiceRhythmConfiguration = new VoiceRhythmConfiguration(Enabled: false)
        };

        var voiceRhythmScheduler = new VoiceRhythmScheduler(compositionConfiguration);

        // act & assert
        for (var measureIndex = 0; measureIndex < 8; measureIndex++)
        {
            voiceRhythmScheduler.TryGetHeldInstrument(measureIndex, out _).Should().BeFalse();
            voiceRhythmScheduler.TryGetFloridInstrument(measureIndex, out _).Should().BeFalse();

            for (var beatIndex = 0; beatIndex < 4; beatIndex++)
            {
                voiceRhythmScheduler.TryGetPinnedInstrument(measureIndex, beatIndex, out _).Should().BeFalse();
            }
        }
    }

    [Test]
    public void TryGetHeldInstrument_WhenHarmonicRhythmIsDisabled_AnswersNothingWhileFloridStaysActive()
    {
        // arrange - the held role's one-attack-per-measure definition is built on the harmonic-rhythm grid
        // (it pins the single interior beat the grid leaves fresh), so disabling the grid disables holds;
        // the florid role reads no grid and stays active.
        var compositionConfiguration = TestCompositionConfigurations.Get() with
        {
            HarmonicRhythmConfiguration = new HarmonicRhythmConfiguration(Enabled: false)
        };

        var voiceRhythmScheduler = new VoiceRhythmScheduler(compositionConfiguration);

        // act & assert
        for (var measureIndex = 0; measureIndex < 8; measureIndex++)
        {
            voiceRhythmScheduler.TryGetHeldInstrument(measureIndex, out _).Should().BeFalse();
            voiceRhythmScheduler.TryGetPinnedInstrument(measureIndex, 2, out _).Should().BeFalse();
            voiceRhythmScheduler.TryGetFloridInstrument(measureIndex, out _).Should().BeTrue();
        }
    }

    [Test]
    public void TryGetHeldInstrument_WithTwoVoices_AnswersNothingWhileFloridAlternates()
    {
        // arrange - holding one of two voices would leave a single moving line rather than a texture
        var voiceRhythmScheduler = new VoiceRhythmScheduler(TestCompositionConfigurations.Get(numberOfInstruments: 2));

        // act & assert
        for (var measureIndex = 0; measureIndex < 8; measureIndex++)
        {
            voiceRhythmScheduler.TryGetHeldInstrument(measureIndex, out _).Should().BeFalse();
            voiceRhythmScheduler.TryGetPinnedInstrument(measureIndex, 2, out _).Should().BeFalse();
        }

        voiceRhythmScheduler.TryGetFloridInstrument(1, out var firstBlockFlorid).Should().BeTrue();
        voiceRhythmScheduler.TryGetFloridInstrument(3, out var secondBlockFlorid).Should().BeTrue();

        firstBlockFlorid.Should().Be(Instrument.Two, "block zero's florid voice is one past the would-be held voice");
        secondBlockFlorid.Should().Be(Instrument.One, "the florid role alternates between the two voices per block");
    }

    [Test]
    public void TryGetFloridInstrument_WithOneVoice_AnswersNothing()
    {
        // arrange
        var voiceRhythmScheduler = new VoiceRhythmScheduler(TestCompositionConfigurations.Get(numberOfInstruments: 1));

        // act & assert
        voiceRhythmScheduler.TryGetFloridInstrument(1, out _).Should().BeFalse();
    }

    [Test]
    public void TryGetHeldInstrument_WithAMinimumPhraseLengthOfOne_NeverHoldsWhileFloridRotatesPerMeasure()
    {
        // arrange - every measure is a seam when the minimum phrase length is one, so holds silently
        // degenerate away while the florid rotation simply advances every measure. The phrasing
        // configuration is constructed fresh rather than via a `with` expression, since MinPhraseLength
        // is computed in a property initializer that non-destructive mutation does not re-run.
        var compositionConfiguration = TestCompositionConfigurations.Get() with
        {
            PhrasingConfiguration = new PhrasingConfiguration(PhraseLengths: [1])
        };

        var voiceRhythmScheduler = new VoiceRhythmScheduler(compositionConfiguration);

        // act & assert
        for (var measureIndex = 0; measureIndex < 8; measureIndex++)
        {
            voiceRhythmScheduler.TryGetHeldInstrument(measureIndex, out _).Should().BeFalse();
        }

        voiceRhythmScheduler.TryGetFloridInstrument(0, out var firstFlorid).Should().BeTrue();
        voiceRhythmScheduler.TryGetFloridInstrument(1, out var secondFlorid).Should().BeTrue();

        firstFlorid.Should().Be(Instrument.Two);
        secondFlorid.Should().Be(Instrument.Three);
    }

    [Test]
    public void TryGetHeldInstrument_WithALongerMinimumPhraseLength_HoldsEveryInteriorMeasureOfTheBlock()
    {
        // arrange
        var compositionConfiguration = TestCompositionConfigurations.Get() with
        {
            PhrasingConfiguration = new PhrasingConfiguration(PhraseLengths: [4, 8])
        };

        var voiceRhythmScheduler = new VoiceRhythmScheduler(compositionConfiguration);

        // act & assert - measures 1-3 are the first block's interior, measure 4 is a seam, measures 5-7
        // belong to the second block whose held voice has rotated
        voiceRhythmScheduler.TryGetHeldInstrument(0, out _).Should().BeFalse();

        for (var measureIndex = 1; measureIndex < 4; measureIndex++)
        {
            voiceRhythmScheduler.TryGetHeldInstrument(measureIndex, out var heldInstrument).Should().BeTrue();
            heldInstrument.Should().Be(Instrument.One);
        }

        voiceRhythmScheduler.TryGetHeldInstrument(4, out _).Should().BeFalse();
        voiceRhythmScheduler.TryGetHeldInstrument(5, out var secondBlockHeld).Should().BeTrue();
        secondBlockHeld.Should().Be(Instrument.Two);
    }

    private static IEnumerable<TestCaseData> HeldRotationTestCases()
    {
        yield return new TestCaseData(0, false, Instrument.One).SetName("A seam measure takes no held voice.");
        yield return new TestCaseData(1, true, Instrument.One).SetName("The first block holds the top voice.");
        yield return new TestCaseData(2, false, Instrument.One).SetName("The next seam measure takes no held voice.");
        yield return new TestCaseData(3, true, Instrument.Two).SetName("The second block rotates to the second voice.");
        yield return new TestCaseData(5, true, Instrument.Three).SetName("The third block rotates to the third voice.");
        yield return new TestCaseData(7, true, Instrument.Four).SetName("The fourth block rotates to the fourth voice.");
        yield return new TestCaseData(9, true, Instrument.One).SetName("The rotation wraps back to the top voice.");
    }

    private static IEnumerable<TestCaseData> PinnedBeatTestCases()
    {
        yield return new TestCaseData(1, 0, false, Instrument.One).SetName("Beat zero is the held voice's one attack, never pinned.");
        yield return new TestCaseData(1, 1, false, Instrument.One).SetName("The grid already holds the second beat, nothing to pin.");
        yield return new TestCaseData(1, 2, true, Instrument.One).SetName("The fresh interior beat pins the held voice.");
        yield return new TestCaseData(1, 3, false, Instrument.One).SetName("The grid already holds the fourth beat, nothing to pin.");
        yield return new TestCaseData(2, 2, false, Instrument.One).SetName("Seam measures pin nothing.");
        yield return new TestCaseData(3, 2, true, Instrument.Two).SetName("The pin follows the block's held voice.");
    }

    private static IEnumerable<TestCaseData> FloridRotationTestCases()
    {
        yield return new TestCaseData(0, Instrument.Two).SetName("The first block's florid voice is one past its held voice.");
        yield return new TestCaseData(1, Instrument.Two).SetName("The florid voice is constant within a block, seams included.");
        yield return new TestCaseData(3, Instrument.Three).SetName("The second block's florid voice rotates with the block.");
        yield return new TestCaseData(7, Instrument.One).SetName("The florid rotation wraps around the voice list.");
    }
}
