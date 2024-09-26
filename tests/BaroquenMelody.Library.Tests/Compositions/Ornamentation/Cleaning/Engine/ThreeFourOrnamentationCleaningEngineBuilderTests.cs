using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.MeterAgnostic;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors.ThreeFour;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Cleaning.Engine;

[TestFixture]
internal sealed class ThreeFourOrnamentationCleaningEngineBuilderTests
{
    private IProcessor<OrnamentationCleaningItem> _mockQuarterHalfOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockEighthNoteOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockHalfEighthOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockSixteenthNoteOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockDelayedRunEighthOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockDoublePassingToneQuarterOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockDoublePassingToneOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockHalfQuarterEighthOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockQuarterQuarterEighthOrnamentationCleaner = null!;

    private IProcessor<OrnamentationCleaningItem> _mockDoublePassingToneDelayedRunOrnamentationCleaner = null!;

    private Dictionary<string, IProcessor<OrnamentationCleaningItem>> _processorsByName = null!;

    private ThreeFourOrnamentationCleaningEngineBuilder _threeFourOrnamentationCleaningEngineBuilder = null!;

    [SetUp]
    public void SetUp()
    {
        _mockQuarterHalfOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockEighthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockHalfEighthOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockSixteenthNoteOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockDelayedRunEighthOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockDoublePassingToneQuarterOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockDoublePassingToneOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockHalfQuarterEighthOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockQuarterQuarterEighthOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();
        _mockDoublePassingToneDelayedRunOrnamentationCleaner = Substitute.For<IProcessor<OrnamentationCleaningItem>>();

        _processorsByName = new Dictionary<string, IProcessor<OrnamentationCleaningItem>>
        {
            { nameof(HalfQuarterOrnamentationCleaner), _mockQuarterHalfOrnamentationCleaner },
            { nameof(EighthNoteOrnamentationCleaner), _mockEighthNoteOrnamentationCleaner },
            { nameof(HalfEighthNoteOrnamentationCleaner), _mockHalfEighthOrnamentationCleaner },
            { nameof(SixteenthNoteOrnamentationCleaner), _mockSixteenthNoteOrnamentationCleaner },
            { nameof(DelayedRunEighthOrnamentationCleaner), _mockDelayedRunEighthOrnamentationCleaner },
            { nameof(DoublePassingToneQuarterOrnamentationCleaner), _mockDoublePassingToneQuarterOrnamentationCleaner },
            { nameof(DoublePassingToneOrnamentationCleaner), _mockDoublePassingToneOrnamentationCleaner },
            { nameof(HalfQuarterEighthOrnamentationCleaner), _mockHalfQuarterEighthOrnamentationCleaner },
            { nameof(QuarterQuarterEighthOrnamentationCleaner), _mockQuarterQuarterEighthOrnamentationCleaner },
            { nameof(DoublePassingToneDelayedRunOrnamentationCleaner), _mockDoublePassingToneDelayedRunOrnamentationCleaner }
        };

        _threeFourOrnamentationCleaningEngineBuilder = new ThreeFourOrnamentationCleaningEngineBuilder(
            _mockQuarterHalfOrnamentationCleaner,
            _mockEighthNoteOrnamentationCleaner,
            _mockHalfEighthOrnamentationCleaner,
            _mockSixteenthNoteOrnamentationCleaner,
            _mockDelayedRunEighthOrnamentationCleaner,
            _mockDoublePassingToneQuarterOrnamentationCleaner,
            _mockDoublePassingToneOrnamentationCleaner,
            _mockHalfQuarterEighthOrnamentationCleaner,
            _mockQuarterQuarterEighthOrnamentationCleaner,
            _mockDoublePassingToneDelayedRunOrnamentationCleaner
        );
    }

    [Test]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.PassingTone, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.DelayedDoublePassingTone, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.NeighborTone, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.RepeatedNote, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.PassingTone, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.Pickup, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.DelayedDoublePassingTone, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.NeighborTone, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.RepeatedNote, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DelayedDoublePassingTone, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.NeighborTone, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.RepeatedNote, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.NeighborTone, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.RepeatedNote, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.RepeatedNote, nameof(HalfQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.Run, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.Turn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.InvertedTurn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.DecorateInterval, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Run, OrnamentationType.Pedal, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Turn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.InvertedTurn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.DecorateInterval, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Turn, OrnamentationType.Pedal, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.InvertedTurn, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.DecorateInterval, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.InvertedTurn, OrnamentationType.Pedal, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.DecorateInterval, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DecorateInterval, OrnamentationType.Pedal, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pedal, OrnamentationType.Pedal, nameof(EighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPassingTone, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedPickup, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedNeighborTone, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPassingTone, OrnamentationType.DelayedRepeatedNote, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPickup, OrnamentationType.DelayedPassingTone, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPickup, OrnamentationType.DelayedPickup, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPickup, OrnamentationType.DelayedNeighborTone, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedPickup, OrnamentationType.DelayedRepeatedNote, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedNeighborTone, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedNeighborTone, OrnamentationType.DelayedRepeatedNote, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedRepeatedNote, OrnamentationType.DelayedRepeatedNote, nameof(HalfEighthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleRun, OrnamentationType.DoubleRun, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleRun, OrnamentationType.DoubleTurn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoubleTurn, OrnamentationType.DoubleTurn, nameof(SixteenthNoteOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedRun, OrnamentationType.Run, nameof(DelayedRunEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedRun, OrnamentationType.Turn, nameof(DelayedRunEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedRun, OrnamentationType.InvertedTurn, nameof(DelayedRunEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedRun, OrnamentationType.DecorateInterval, nameof(DelayedRunEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedRun, OrnamentationType.Pedal, nameof(DelayedRunEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.PassingTone, nameof(DoublePassingToneQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Pickup, nameof(DoublePassingToneQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.RepeatedNote, nameof(DoublePassingToneQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.NeighborTone, nameof(DoublePassingToneQuarterOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.DoublePassingTone, nameof(DoublePassingToneOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Run, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Turn, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.InvertedTurn, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.DecorateInterval, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.PassingTone, OrnamentationType.Pedal, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.Run, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.Turn, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.InvertedTurn, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.DecorateInterval, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.Pickup, OrnamentationType.Pedal, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.Run, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.Turn, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.InvertedTurn, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.DecorateInterval, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, OrnamentationType.Pedal, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.Run, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.Turn, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.InvertedTurn, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.DecorateInterval, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.RepeatedNote, OrnamentationType.Pedal, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.Run, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.Turn, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.InvertedTurn, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.DecorateInterval, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.NeighborTone, OrnamentationType.Pedal, nameof(HalfQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Run, nameof(QuarterQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Turn, nameof(QuarterQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.InvertedTurn, nameof(QuarterQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.DecorateInterval, nameof(QuarterQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.Pedal, nameof(QuarterQuarterEighthOrnamentationCleaner))]
    [TestCase(OrnamentationType.DoublePassingTone, OrnamentationType.DelayedRun, nameof(DoublePassingToneDelayedRunOrnamentationCleaner))]
    public void Process_invokes_expected_ornamentation_cleaner(OrnamentationType ornamentationTypeA, OrnamentationType ornamentationTypeB, string expectedProcessorName)
    {
        // arrange
        var ornamentationCleaningItem = new OrnamentationCleaningItem(
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half) { OrnamentationType = ornamentationTypeA },
            new BaroquenNote(Instrument.Two, Notes.C3, MusicalTimeSpan.Half) { OrnamentationType = ornamentationTypeB }
        );

        var ornamentationCleaningEngine = _threeFourOrnamentationCleaningEngineBuilder.Build();

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
