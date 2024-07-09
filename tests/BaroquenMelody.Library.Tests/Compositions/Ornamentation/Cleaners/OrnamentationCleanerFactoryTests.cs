﻿using BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaners;

[TestFixture]
internal sealed class OrnamentationCleanerFactoryTests
{
    private OrnamentationCleanerFactory _factory;

    [SetUp]
    public void SetUp() => _factory = new OrnamentationCleanerFactory();

    [Test]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.PassingTone, typeof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone, typeof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone, typeof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone, typeof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone, typeof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedDoublePassingTone, typeof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone, typeof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone, typeof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.SixteenthNoteRun, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.Turn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.SixteenthNoteRun, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.AlternateTurn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.SixteenthNoteRun, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Turn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.AlternateTurn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DecorateInterval, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.SixteenthNoteRun, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Turn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.AlternateTurn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.DecorateInterval, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DecorateInterval, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.DecorateInterval, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Pedal, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.SixteenthNoteRun, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Turn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.AlternateTurn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.DecorateInterval, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.Pedal, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Pedal, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.Pedal, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Pedal, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.SixteenthNoteRun, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.SixteenthNoteRun, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.PassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.DoublePassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.PassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DoublePassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Turn, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Turn, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.AlternateTurn, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.AlternateTurn, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.PassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.DoublePassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.DecorateInterval, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.PassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.DecorateInterval, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DoublePassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.PassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.DoublePassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Pedal, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Pedal, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.AlternateTurn, typeof(TurnAlternateTurnOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.Turn, typeof(TurnAlternateTurnOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.ThirtySecondNoteRun, typeof(ThirtySecondNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn, typeof(ThirtySecondNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.DoubleTurn, typeof(ThirtySecondNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.ThirtySecondNoteRun, typeof(ThirtySecondNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.SixteenthNoteRun, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.ThirtySecondNoteRun, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.Turn, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.ThirtySecondNoteRun, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.AlternateTurn, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.ThirtySecondNoteRun, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.DecorateInterval, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.ThirtySecondNoteRun, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.SixteenthNoteRun, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.DoubleTurn, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.Turn, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DoubleTurn, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.AlternateTurn, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.DoubleTurn, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.DecorateInterval, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DoubleTurn, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.Pedal, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.ThirtySecondNoteRun, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.Pedal, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.DoubleTurn, typeof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Sustain, OrnamentationType.Rest, typeof(NoOpOrnamentationCleaner))]
    public void Get_Returns_Expected_OrnamentationCleaner(OrnamentationType ornamentationTypeA, OrnamentationType ornamentationTypeB, Type expectedType)
    {
        // act
        var cleaner = _factory.Get(ornamentationTypeA, ornamentationTypeB);

        // assert
        cleaner.Should().BeOfType(expectedType);
    }
}
