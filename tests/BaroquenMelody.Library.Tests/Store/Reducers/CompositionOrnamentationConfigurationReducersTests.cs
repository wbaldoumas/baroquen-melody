using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Reducers;

[TestFixture]
internal sealed class CompositionOrnamentationConfigurationReducersTests
{
    [Test]
    public void ReduceUpdateCompositionOrnamentationConfiguration_updates_composition_ornamentation_configurations_as_expected()
    {
        // arrange
        var state = new CompositionOrnamentationConfigurationState();

        // act
        state = CompositionOrnamentationConfigurationReducers.ReduceUpdateCompositionOrnamentationConfiguration(
            state,
            new UpdateCompositionOrnamentationConfiguration(OrnamentationType.Run, ConfigurationStatus.Enabled, 1)
        );

        state = CompositionOrnamentationConfigurationReducers.ReduceUpdateCompositionOrnamentationConfiguration(
            state,
            new UpdateCompositionOrnamentationConfiguration(OrnamentationType.Mordent, ConfigurationStatus.Enabled, 2)
        );

        state = CompositionOrnamentationConfigurationReducers.ReduceUpdateCompositionOrnamentationConfiguration(
            state,
            new UpdateCompositionOrnamentationConfiguration(OrnamentationType.Turn, ConfigurationStatus.Enabled, 3)
        );

        state = CompositionOrnamentationConfigurationReducers.ReduceUpdateCompositionOrnamentationConfiguration(
            state,
            new UpdateCompositionOrnamentationConfiguration(OrnamentationType.Mordent, ConfigurationStatus.Enabled, 4)
        );

        // assert
        state.Configurations
            .Should()
            .ContainKeys(OrnamentationType.Run, OrnamentationType.Turn, OrnamentationType.Mordent);

        state[OrnamentationType.Run]!.Probability.Should().Be(1);
        state[OrnamentationType.Turn]!.Probability.Should().Be(3);
        state[OrnamentationType.Mordent]!.Probability.Should().Be(4);

        var otherConfigurations = AggregateOrnamentationConfiguration.Default.Configurations
            .Where(configuration => configuration.OrnamentationType
                is not OrnamentationType.Run
                and not OrnamentationType.Turn
                and not OrnamentationType.Mordent
            );

        foreach (var defaultConfiguration in otherConfigurations)
        {
            state[defaultConfiguration.OrnamentationType]!.Should().BeEquivalentTo(defaultConfiguration);
        }
    }
}
