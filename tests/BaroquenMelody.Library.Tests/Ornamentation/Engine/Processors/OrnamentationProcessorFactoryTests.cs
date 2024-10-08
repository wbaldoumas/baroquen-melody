using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Ornamentation.Engine.Processors;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Ornamentation.Utilities;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using NUnit.Framework;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Processors;

[TestFixture]
internal sealed class OrnamentationProcessorFactoryTests
{
    private OrnamentationProcessorFactory _ornamentationProcessorFactory = null!;

    [SetUp]
    public void SetUp()
    {
        _ornamentationProcessorFactory = new OrnamentationProcessorFactory(
            new MusicalTimeSpanCalculator(),
            new WeightedRandomBooleanGenerator()
        );
    }

    [Test]
    [TestCase(OrnamentationType.PassingTone, typeof(PassingToneProcessor))]
    [TestCase(OrnamentationType.Run, typeof(RunProcessor))]
    [TestCase(OrnamentationType.DelayedPassingTone, typeof(PassingToneProcessor))]
    [TestCase(OrnamentationType.Turn, typeof(TurnProcessor))]
    [TestCase(OrnamentationType.InvertedTurn, typeof(InvertedTurnProcessor))]
    [TestCase(OrnamentationType.DelayedRun, typeof(DelayedRunProcessor))]
    [TestCase(OrnamentationType.DoubleTurn, typeof(DoubleTurnProcessor))]
    [TestCase(OrnamentationType.DoublePassingTone, typeof(DoublePassingToneProcessor))]
    [TestCase(OrnamentationType.DelayedDoublePassingTone, typeof(DoublePassingToneProcessor))]
    [TestCase(OrnamentationType.DecorateInterval, typeof(DecorateIntervalProcessor))]
    [TestCase(OrnamentationType.DoubleRun, typeof(DoubleRunProcessor))]
    [TestCase(OrnamentationType.Pedal, typeof(PedalProcessor))]
    [TestCase(OrnamentationType.Mordent, typeof(MordentProcessor))]
    [TestCase(OrnamentationType.RepeatedNote, typeof(RepeatedNoteProcessor))]
    [TestCase(OrnamentationType.DelayedRepeatedNote, typeof(RepeatedNoteProcessor))]
    [TestCase(OrnamentationType.NeighborTone, typeof(NeighborToneProcessor))]
    [TestCase(OrnamentationType.DelayedNeighborTone, typeof(NeighborToneProcessor))]
    [TestCase(OrnamentationType.Pickup, typeof(PickupProcessor))]
    [TestCase(OrnamentationType.DelayedPickup, typeof(PickupProcessor))]
    public void Factory_returns_expected_processor_for_ornamentation_type(OrnamentationType ornamentationType, Type expectedProcessorType)
    {
        // act
        var processor = _ornamentationProcessorFactory.Create(ornamentationType, TestCompositionConfigurations.Get());

        // assert
        processor.Should().BeOfType(expectedProcessorType);
    }

    [Test]
    [TestCase(OrnamentationType.Rest)]
    [TestCase(OrnamentationType.None)]
    [TestCase(OrnamentationType.Sustain)]
    [TestCase(OrnamentationType.MidSustain)]
    [TestCase((OrnamentationType)byte.MaxValue)]
    public void Factory_throws_for_unsupported_ornamentation_type(OrnamentationType ornamentationType)
    {
        // act
        var act = () => _ornamentationProcessorFactory.Create(ornamentationType, TestCompositionConfigurations.Get());

        // assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Factory_handles_all_ornamentation_types()
    {
        // arrange
        var excludedTypes = new HashSet<OrnamentationType>
        {
            OrnamentationType.Rest,
            OrnamentationType.None,
            OrnamentationType.Sustain,
            OrnamentationType.MidSustain
        }.ToFrozenSet();

        var ornamentationTypes = EnumUtils<OrnamentationType>
            .AsEnumerable()
            .Where(ornamentationType => !excludedTypes.Contains(ornamentationType));

        // act
        foreach (var ornamentationType in ornamentationTypes)
        {
            var act = () => _ornamentationProcessorFactory.Create(ornamentationType, TestCompositionConfigurations.Get());

            // assert
            act.Should().NotThrow();
        }
    }
}
