﻿using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.FourFour;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.MeterAgnostic;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine;

[TestFixture]
internal sealed class FourFourOrnamentationCleaningEngineBuilderTests
{
    private IProcessor<OrnamentationCleaningItem> _mockPassingToneOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockEighthNoteOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockPassingToneEighthNoteOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockTurnInvertedTurnOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockSixteenthNoteOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockSixteenthEighthNoteOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockMordentEighthNoteOrnamentationCleaner = null!;

    private Dictionary<string, IProcessor<OrnamentationCleaningItem>> _processorsByName = null!;

    private FourFourOrnamentationCleaningEngineBuilder _fourFourOrnamentationCleaningEngineBuilder;

    [SetUp]
    public void SetUp()
    {
        _mockPassingToneOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockEighthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockPassingToneEighthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockTurnInvertedTurnOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockSixteenthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockSixteenthEighthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockMordentEighthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();

        _processorsByName = new Dictionary<string, IProcessor<OrnamentationCleaningItem>>
        {
            { nameof(QuarterNoteOrnamentationCleaner), _mockPassingToneOrnamentationCleaner },
            { nameof(EighthNoteOrnamentationCleaner), _mockEighthNoteOrnamentationCleaner },
            { nameof(QuarterEighthNoteOrnamentationCleaner), _mockPassingToneEighthNoteOrnamentationCleaner },
            { nameof(TurnInvertedTurnOrnamentationCleaner), _mockTurnInvertedTurnOrnamentationCleaner },
            { nameof(SixteenthNoteOrnamentationCleaner), _mockSixteenthNoteOrnamentationCleaner },
            { nameof(SixteenthEighthNoteOrnamentationCleaner), _mockSixteenthEighthNoteOrnamentationCleaner },
            { nameof(MordentEighthNoteOrnamentationCleaner), _mockMordentEighthNoteOrnamentationCleaner }
        };

        _fourFourOrnamentationCleaningEngineBuilder = new FourFourOrnamentationCleaningEngineBuilder(
            _mockPassingToneOrnamentationCleaner,
            _mockEighthNoteOrnamentationCleaner,
            _mockPassingToneEighthNoteOrnamentationCleaner,
            _mockTurnInvertedTurnOrnamentationCleaner,
            _mockSixteenthNoteOrnamentationCleaner,
            _mockSixteenthEighthNoteOrnamentationCleaner,
            _mockMordentEighthNoteOrnamentationCleaner
        );
    }

    [Test]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.PassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.Pickup, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.PassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.DoublePassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.DoublePassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Pickup, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.RepeatedNote, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.RepeatedNote, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.PassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.DoublePassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.Pickup, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.RepeatedNote, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedDoublePassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPickup, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPickup, OrnamentationType.DelayedPassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPickup, OrnamentationType.DelayedDoublePassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPickup, OrnamentationType.DelayedPickup, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedPickup, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedPassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedPickup, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedDoublePassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedRepeatedNote, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedRepeatedNote, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPickup, OrnamentationType.DelayedRepeatedNote, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedNeighborTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedPassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedPickup, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedDoublePassingTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedRepeatedNote, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedNeighborTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPickup, OrnamentationType.DelayedNeighborTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedNeighborTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedNeighborTone, nameof(QuarterNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.Run, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.Turn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Run, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.InvertedTurn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.Run, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Turn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.InvertedTurn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DecorateInterval, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Run, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Turn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.InvertedTurn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.DecorateInterval, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DecorateInterval, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.DecorateInterval, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Pedal, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Run, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Turn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.InvertedTurn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.DecorateInterval, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.Pedal, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Pedal, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.Pedal, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Pedal, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Run, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.Run, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Run, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.PassingTone, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.Pickup, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.DoublePassingTone, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.PassingTone, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Pickup, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DoublePassingTone, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Turn, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.Turn, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Turn, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.InvertedTurn, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.InvertedTurn, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.InvertedTurn, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.PassingTone, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.Pickup, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.DoublePassingTone, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.DecorateInterval, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.DecorateInterval, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.PassingTone, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Pickup, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.DecorateInterval, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DoublePassingTone, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.PassingTone, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Pickup, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.DoublePassingTone, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Pedal, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.Pedal, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Pedal, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.RepeatedNote, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.RepeatedNote, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.RepeatedNote, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.RepeatedNote, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.RepeatedNote, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.Pedal, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.DecorateInterval, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.InvertedTurn, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.Turn, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.Run, nameof(QuarterEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.InvertedTurn, nameof(TurnInvertedTurnOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.Turn, nameof(TurnInvertedTurnOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleRun, OrnamentationType.DoubleRun, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleRun, OrnamentationType.DoubleTurn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.DoubleRun, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleRun, OrnamentationType.Run, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.DoubleRun, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleRun, OrnamentationType.Turn, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DoubleRun, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleRun, OrnamentationType.InvertedTurn, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.DoubleRun, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleRun, OrnamentationType.DecorateInterval, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DoubleRun, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.Run, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.DoubleTurn, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.Turn, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DoubleTurn, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.InvertedTurn, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.DoubleTurn, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.DecorateInterval, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DoubleTurn, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleRun, OrnamentationType.Pedal, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.DoubleRun, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.Pedal, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.DoubleTurn, nameof(SixteenthEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Mordent, OrnamentationType.Run, nameof(MordentEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.Mordent, nameof(MordentEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Mordent, OrnamentationType.Turn, nameof(MordentEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Mordent, nameof(MordentEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Mordent, OrnamentationType.InvertedTurn, nameof(MordentEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.Mordent, nameof(MordentEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Mordent, OrnamentationType.DecorateInterval, nameof(MordentEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Mordent, nameof(MordentEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Mordent, OrnamentationType.Pedal, nameof(MordentEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Mordent, nameof(MordentEighthNoteOrnamentationCleaner))]
    public void Process_invokes_expected_ornamentation_cleaner(OrnamentationType ornamentationTypeA, OrnamentationType ornamentationTypeB, string expectedProcessorName)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { OrnamentationType = ornamentationTypeA },
            new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half) { OrnamentationType = ornamentationTypeB }
        );

        var ornamentationCleaningEngine = _fourFourOrnamentationCleaningEngineBuilder.Build();

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
