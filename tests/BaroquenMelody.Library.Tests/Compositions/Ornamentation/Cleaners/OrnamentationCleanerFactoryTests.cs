using BaroquenMelody.Library.Compositions.Ornamentation.Cleaners;
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
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone, typeof(PassingToneOrnamentationCleaner))]

    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.SixteenthNoteRun, typeof(SixteenthNoteOrnamentationCleaner))]

    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.Turn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.SixteenthNoteRun, typeof(SixteenthNoteOrnamentationCleaner))]

    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.AlternateTurn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.SixteenthNoteRun, typeof(SixteenthNoteOrnamentationCleaner))]

    [TestCase(OrnamentationType.Turn, OrnamentationType.Turn, typeof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.AlternateTurn, typeof(SixteenthNoteOrnamentationCleaner))]

    [TestCase(OrnamentationType.PassingTone, OrnamentationType.SixteenthNoteRun, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.PassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.PassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Turn, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.AlternateTurn, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.PassingTone, typeof(PassingToneSixteenthNoteOrnamentationCleaner))]

    [TestCase(OrnamentationType.Turn, OrnamentationType.AlternateTurn, typeof(TurnAlternateTurnOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.Turn, typeof(TurnAlternateTurnOrnamentationCleaner))]

    [TestCase(OrnamentationType.Sustain, OrnamentationType.Rest, typeof(NoOpOrnamentationCleaner))]
    public void Get_Returns_Expected_OrnamentationCleaner(OrnamentationType ornamentationTypeA, OrnamentationType ornamentationTypeB, Type expectedType)
    {
        // act
        var cleaner = _factory.Get(ornamentationTypeA, ornamentationTypeB);

        // assert
        cleaner.Should().BeOfType(expectedType);
    }
}
