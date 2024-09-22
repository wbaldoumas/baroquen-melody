using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Configurations.Services;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Fluxor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Configuration.Services;

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

        _mockState.Value.Returns(new CompositionOrnamentationConfigurationState());

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
            OrnamentationType.DelayedPassingTone,
            OrnamentationType.DelayedNeighborTone,
            OrnamentationType.NeighborTone,
            OrnamentationType.Run,
            OrnamentationType.DoubleRun,
            OrnamentationType.Turn,
            OrnamentationType.AlternateTurn,
            OrnamentationType.DelayedRun,
            OrnamentationType.Mordent,
            OrnamentationType.DecorateInterval,
            OrnamentationType.Pedal,
            OrnamentationType.RepeatedNote,
            OrnamentationType.DelayedRepeatedNote,
            OrnamentationType.Pickup,
            OrnamentationType.DelayedPickup
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
            .Received(AggregateOrnamentationConfiguration.Default.Configurations.Count)
            .Dispatch(Arg.Any<UpdateCompositionOrnamentationConfiguration>());
    }

    [Test]
    public void Randomize_dispatches_expected_actions()
    {
        // act
        _ornamentationConfigurationService.Randomize();

        // assert
        _mockDispatcher
            .Received(AggregateOrnamentationConfiguration.Default.Configurations.Count)
            .Dispatch(Arg.Any<UpdateCompositionOrnamentationConfiguration>());
    }
}
