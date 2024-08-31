using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;
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
            new UpdateCompositionOrnamentationConfiguration(OrnamentationType.Run, true, 1)
        );

        state = CompositionOrnamentationConfigurationReducers.ReduceUpdateCompositionOrnamentationConfiguration(
            state,
            new UpdateCompositionOrnamentationConfiguration(OrnamentationType.Mordent, true, 2)
        );

        state = CompositionOrnamentationConfigurationReducers.ReduceUpdateCompositionOrnamentationConfiguration(
            state,
            new UpdateCompositionOrnamentationConfiguration(OrnamentationType.Turn, true, 3)
        );

        state = CompositionOrnamentationConfigurationReducers.ReduceUpdateCompositionOrnamentationConfiguration(
            state,
            new UpdateCompositionOrnamentationConfiguration(OrnamentationType.Mordent, true, 4)
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
