using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Fluxor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configuration.Services;

[TestFixture]
internal sealed class OrnamentationConfigurationServiceTests
{
    private IDispatcher _mockDispatcher = null!;

    private IState<CompositionOrnamentationConfigurationState> _mockState = null!;

    private OrnamentationConfigurationService _ornamentationConfigurationService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockState = Substitute.For<IState<CompositionOrnamentationConfigurationState>>();

        _ornamentationConfigurationService = new OrnamentationConfigurationService(_mockDispatcher, _mockState);
    }

    [Test]
    public void ConfigurableOrnamentations_returns_expected_values()
    {
        // arrange
        var expectedConfigurableOrnamentations = new[]
        {
            OrnamentationType.PassingTone,
            OrnamentationType.DoublePassingTone,
            OrnamentationType.DelayedDoublePassingTone,
            OrnamentationType.DoubleTurn,
            OrnamentationType.DoubleInvertedTurn,
            OrnamentationType.DelayedPassingTone,
            OrnamentationType.DelayedNeighborTone,
            OrnamentationType.NeighborTone,
            OrnamentationType.Run,
            OrnamentationType.DoubleRun,
            OrnamentationType.Turn,
            OrnamentationType.InvertedTurn,
            OrnamentationType.DelayedRun,
            OrnamentationType.Mordent,
            OrnamentationType.DecorateInterval,
            OrnamentationType.Pedal,
            OrnamentationType.RepeatedNote,
            OrnamentationType.DelayedRepeatedNote,
            OrnamentationType.Pickup,
            OrnamentationType.DelayedPickup,
            OrnamentationType.DoublePickup,
            OrnamentationType.DelayedDoublePickup,
            OrnamentationType.DecorateThird,
            OrnamentationType.OctavePedal,
            OrnamentationType.OctavePedalPassingTone,
            OrnamentationType.OctavePedalArpeggio,
            OrnamentationType.UpperOctavePedal,
            OrnamentationType.UpperOctavePedalPassingTone,
            OrnamentationType.UpperOctavePedalArpeggio,
            OrnamentationType.TriplePickup,
            OrnamentationType.SequencedThirds
        };

        // act
        var actualConfigurableOrnamentations = _ornamentationConfigurationService.ConfigurableOrnamentations;

        // assert
        actualConfigurableOrnamentations.Order().Should().BeEquivalentTo(expectedConfigurableOrnamentations.Order());
    }

    [Test]
    public void ConfigureDefaults_dispatches_expected_actions()
    {
        // act
        _ornamentationConfigurationService.ConfigureDefaults();

        // assert
        _mockDispatcher
            .Received(1)
            .Dispatch(Arg.Any<BatchUpdateCompositionOrnamentationConfiguration>());
    }

    [Test]
    public void Randomize_dispatches_expected_actions()
    {
        // arrange
        var configurations = new Dictionary<OrnamentationType, OrnamentationConfiguration>
        {
            { OrnamentationType.PassingTone, new OrnamentationConfiguration(OrnamentationType.PassingTone, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DoublePassingTone, new OrnamentationConfiguration(OrnamentationType.DoublePassingTone, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DelayedDoublePassingTone, new OrnamentationConfiguration(OrnamentationType.DelayedDoublePassingTone, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DoubleTurn, new OrnamentationConfiguration(OrnamentationType.DoubleInvertedTurn, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DoubleInvertedTurn, new OrnamentationConfiguration(OrnamentationType.DoubleInvertedTurn, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DelayedPassingTone, new OrnamentationConfiguration(OrnamentationType.DelayedPassingTone, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DelayedNeighborTone, new OrnamentationConfiguration(OrnamentationType.DelayedNeighborTone, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.NeighborTone, new OrnamentationConfiguration(OrnamentationType.NeighborTone, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.Run, new OrnamentationConfiguration(OrnamentationType.Run, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DoubleRun, new OrnamentationConfiguration(OrnamentationType.DoubleRun, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.Turn, new OrnamentationConfiguration(OrnamentationType.Turn, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.InvertedTurn, new OrnamentationConfiguration(OrnamentationType.InvertedTurn, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DelayedRun, new OrnamentationConfiguration(OrnamentationType.DelayedRun, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.Mordent, new OrnamentationConfiguration(OrnamentationType.Mordent, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DecorateInterval, new OrnamentationConfiguration(OrnamentationType.DecorateInterval, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.Pedal, new OrnamentationConfiguration(OrnamentationType.Pedal, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.RepeatedNote, new OrnamentationConfiguration(OrnamentationType.RepeatedNote, ConfigurationStatus.Disabled, 100) },
            { OrnamentationType.DelayedRepeatedNote, new OrnamentationConfiguration(OrnamentationType.DelayedRepeatedNote, ConfigurationStatus.Locked, 100) },
            { OrnamentationType.Pickup, new OrnamentationConfiguration(OrnamentationType.Pickup, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DelayedPickup, new OrnamentationConfiguration(OrnamentationType.DelayedPickup, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DoublePickup, new OrnamentationConfiguration(OrnamentationType.DoublePickup, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DelayedDoublePickup, new OrnamentationConfiguration(OrnamentationType.DoublePickup, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.DecorateThird, new OrnamentationConfiguration(OrnamentationType.DecorateThird, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.OctavePedal, new OrnamentationConfiguration(OrnamentationType.OctavePedal, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.OctavePedalPassingTone, new OrnamentationConfiguration(OrnamentationType.OctavePedalPassingTone, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.OctavePedalArpeggio, new OrnamentationConfiguration(OrnamentationType.OctavePedalArpeggio, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.UpperOctavePedal, new OrnamentationConfiguration(OrnamentationType.UpperOctavePedal, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.UpperOctavePedalPassingTone, new OrnamentationConfiguration(OrnamentationType.UpperOctavePedalPassingTone, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.UpperOctavePedalArpeggio, new OrnamentationConfiguration(OrnamentationType.UpperOctavePedalArpeggio, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.TriplePickup, new OrnamentationConfiguration(OrnamentationType.TriplePickup, ConfigurationStatus.Enabled, 100) },
            { OrnamentationType.SequencedThirds, new OrnamentationConfiguration(OrnamentationType.SequencedThirds, ConfigurationStatus.Enabled, 100) }
        };

        _mockState.Value.Returns(new CompositionOrnamentationConfigurationState(configurations));

        // act
        _ornamentationConfigurationService.Randomize();

        // assert
        _mockDispatcher
            .Received(1)
            .Dispatch(Arg.Any<BatchUpdateCompositionOrnamentationConfiguration>());
    }
}
