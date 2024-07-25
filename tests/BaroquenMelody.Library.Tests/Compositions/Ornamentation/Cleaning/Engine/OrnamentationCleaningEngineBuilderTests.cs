using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine;

[TestFixture]
internal sealed class OrnamentationCleaningEngineBuilderTests
{
    private IProcessor<OrnamentationCleaningItem> _mockPassingToneOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockSixteenthNoteOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockPassingToneSixteenthNoteOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockTurnAlternateTurnOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockThirtySecondNoteOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockThirtySecondSixteenthNoteOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockMordentSixteenthNoteOrnamentationCleaner = null!;

    private Dictionary<string, IProcessor<OrnamentationCleaningItem>> _processorsByName = null!;

    private OrnamentationCleaningEngineBuilder _ornamentationCleaningEngineBuilder;

    [SetUp]
    public void SetUp()
    {
        _mockPassingToneOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockSixteenthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockPassingToneSixteenthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockTurnAlternateTurnOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockThirtySecondNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockThirtySecondSixteenthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockMordentSixteenthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();

        _processorsByName = new Dictionary<string, IProcessor<OrnamentationCleaningItem>>
        {
            { nameof(PassingToneOrnamentationCleaner), _mockPassingToneOrnamentationCleaner },
            { nameof(SixteenthNoteOrnamentationCleaner), _mockSixteenthNoteOrnamentationCleaner },
            { nameof(EighthSixteenthNoteOrnamentationCleaner), _mockPassingToneSixteenthNoteOrnamentationCleaner },
            { nameof(TurnAlternateTurnOrnamentationCleaner), _mockTurnAlternateTurnOrnamentationCleaner },
            { nameof(ThirtySecondNoteOrnamentationCleaner), _mockThirtySecondNoteOrnamentationCleaner },
            { nameof(ThirtySecondSixteenthNoteOrnamentationCleaner), _mockThirtySecondSixteenthNoteOrnamentationCleaner },
            { nameof(MordentSixteenthNoteOrnamentationCleaner), _mockMordentSixteenthNoteOrnamentationCleaner }
        };

        _ornamentationCleaningEngineBuilder = new OrnamentationCleaningEngineBuilder(
            _mockPassingToneOrnamentationCleaner,
            _mockSixteenthNoteOrnamentationCleaner,
            _mockPassingToneSixteenthNoteOrnamentationCleaner,
            _mockTurnAlternateTurnOrnamentationCleaner,
            _mockThirtySecondNoteOrnamentationCleaner,
            _mockThirtySecondSixteenthNoteOrnamentationCleaner,
            _mockMordentSixteenthNoteOrnamentationCleaner
        );
    }

    [Test]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.PassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.RepeatedEighthNote, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedEighthNote, OrnamentationType.PassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedEighthNote, OrnamentationType.DoublePassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.RepeatedEighthNote, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedDoublePassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.DelayedPassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.DelayedDoublePassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.RepeatedDottedEighthSixteenth, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.RepeatedDottedEighthSixteenth, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.NeighborTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.DelayedPassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.DelayedDoublePassingTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.RepeatedDottedEighthSixteenth, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.NeighborTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.NeighborTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedDottedEighthSixteenth, OrnamentationType.NeighborTone, nameof(PassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.SixteenthNoteRun, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.Turn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.SixteenthNoteRun, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.AlternateTurn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.SixteenthNoteRun, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Turn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.AlternateTurn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DecorateInterval, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.SixteenthNoteRun, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Turn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.AlternateTurn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.DecorateInterval, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DecorateInterval, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.DecorateInterval, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Pedal, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.SixteenthNoteRun, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Turn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.AlternateTurn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.DecorateInterval, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.Pedal, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Pedal, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.Pedal, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Pedal, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.SixteenthNoteRun, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.SixteenthNoteRun, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.PassingTone, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.DoublePassingTone, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.PassingTone, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DoublePassingTone, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Turn, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Turn, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.AlternateTurn, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.AlternateTurn, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.PassingTone, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.DoublePassingTone, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.DecorateInterval, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.PassingTone, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.DecorateInterval, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DoublePassingTone, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.PassingTone, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.DoublePassingTone, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Pedal, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Pedal, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.RepeatedEighthNote, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.RepeatedEighthNote, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.RepeatedEighthNote, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.RepeatedEighthNote, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.RepeatedEighthNote, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedEighthNote, OrnamentationType.Pedal, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedEighthNote, OrnamentationType.DecorateInterval, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedEighthNote, OrnamentationType.AlternateTurn, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedEighthNote, OrnamentationType.Turn, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedEighthNote, OrnamentationType.SixteenthNoteRun, nameof(EighthSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.AlternateTurn, nameof(TurnAlternateTurnOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.Turn, nameof(TurnAlternateTurnOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.ThirtySecondNoteRun, nameof(ThirtySecondNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn, nameof(ThirtySecondNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.DoubleTurn, nameof(ThirtySecondNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.ThirtySecondNoteRun, nameof(ThirtySecondNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.SixteenthNoteRun, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.ThirtySecondNoteRun, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.Turn, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.ThirtySecondNoteRun, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.AlternateTurn, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.ThirtySecondNoteRun, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.DecorateInterval, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.ThirtySecondNoteRun, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.SixteenthNoteRun, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.DoubleTurn, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.Turn, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DoubleTurn, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.AlternateTurn, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.DoubleTurn, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.DecorateInterval, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DoubleTurn, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.ThirtySecondNoteRun, OrnamentationType.Pedal, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.ThirtySecondNoteRun, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.Pedal, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.DoubleTurn, nameof(ThirtySecondSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Mordent, OrnamentationType.SixteenthNoteRun, nameof(MordentSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.SixteenthNoteRun, OrnamentationType.Mordent, nameof(MordentSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Mordent, OrnamentationType.Turn, nameof(MordentSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Mordent, nameof(MordentSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Mordent, OrnamentationType.AlternateTurn, nameof(MordentSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.AlternateTurn, OrnamentationType.Mordent, nameof(MordentSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Mordent, OrnamentationType.DecorateInterval, nameof(MordentSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Mordent, nameof(MordentSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Mordent, OrnamentationType.Pedal, nameof(MordentSixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Mordent, nameof(MordentSixteenthNoteOrnamentationCleaner))]
    public void Process_invokes_expected_ornamentation_cleaner(OrnamentationType ornamentationTypeA, OrnamentationType ornamentationTypeB, string expectedProcessorName)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(
            new BaroquenNote(Voice.Soprano, Notes.C4) { OrnamentationType = ornamentationTypeA },
            new BaroquenNote(Voice.Alto, Notes.C3) { OrnamentationType = ornamentationTypeB }
        );

        var ornamentationCleaningEngine = _ornamentationCleaningEngineBuilder.Build();

        // act
        ornamentationCleaningEngine.Process(ornamentationCleaningItem);

        // assert
        _processorsByName[expectedProcessorName].Received(1).Process(ornamentationCleaningItem);

        _processorsByName.Keys
            .Where(key => key != expectedProcessorName)
            .ToList()
            .ForEach(key => _processorsByName[key].DidNotReceive().Process(ornamentationCleaningItem));
    }
}
