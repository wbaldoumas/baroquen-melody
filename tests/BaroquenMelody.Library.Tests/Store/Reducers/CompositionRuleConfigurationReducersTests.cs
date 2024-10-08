using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Reducers;

[TestFixture]
internal sealed class CompositionRuleConfigurationReducersTests
{
    [Test]
    public void ReduceUpdateCompositionRuleConfiguration_updates_composition_rule_configurations_as_expected()
    {
        // arrange
        var state = new CompositionRuleConfigurationState();

        // act
        state = CompositionRuleConfigurationReducers.ReduceUpdateCompositionRuleConfiguration(state, new UpdateCompositionRuleConfiguration(CompositionRule.AvoidDirectFifths, ConfigurationStatus.Enabled, 1));
        state = CompositionRuleConfigurationReducers.ReduceUpdateCompositionRuleConfiguration(state, new UpdateCompositionRuleConfiguration(CompositionRule.AvoidDissonance, ConfigurationStatus.Enabled, 2));
        state = CompositionRuleConfigurationReducers.ReduceUpdateCompositionRuleConfiguration(state, new UpdateCompositionRuleConfiguration(CompositionRule.AvoidRepeatedChords, ConfigurationStatus.Enabled, 3));
        state = CompositionRuleConfigurationReducers.ReduceUpdateCompositionRuleConfiguration(state, new UpdateCompositionRuleConfiguration(CompositionRule.AvoidDirectFifths, ConfigurationStatus.Disabled, 4));

        // assert
        state.Configurations
            .Should()
            .ContainKeys(CompositionRule.AvoidDirectFifths, CompositionRule.AvoidDissonance, CompositionRule.AvoidRepeatedChords);

        state[CompositionRule.AvoidDirectFifths]!.Strictness.Should().Be(4);
        state[CompositionRule.AvoidDirectFifths]!.IsEnabled.Should().BeFalse();
        state[CompositionRule.AvoidDissonance]!.Strictness.Should().Be(2);
        state[CompositionRule.AvoidDissonance]!.IsEnabled.Should().BeTrue();
        state[CompositionRule.AvoidRepeatedChords]!.Strictness.Should().Be(3);
        state[CompositionRule.AvoidRepeatedChords]!.IsEnabled.Should().BeTrue();

        var otherConfigurations = AggregateCompositionRuleConfiguration.Default.Configurations
            .Where(configuration => configuration.Rule
                is not CompositionRule.AvoidDirectFifths
                and not CompositionRule.AvoidDissonance
                and not CompositionRule.AvoidRepeatedChords
            );

        foreach (var defaultConfiguration in otherConfigurations)
        {
            state[defaultConfiguration.Rule]!.Should().BeEquivalentTo(defaultConfiguration);
        }
    }

    [Test]
    public void ReduceBatchUpdateCompositionRuleConfiguration_updates_composition_rule_configurations_as_expected()
    {
        // arrange
        var state = new CompositionRuleConfigurationState();

        // act
        state = CompositionRuleConfigurationReducers.ReduceBatchUpdateCompositionRuleConfiguration(
            state,
            new BatchUpdateCompositionRuleConfiguration(
                new Dictionary<CompositionRule, CompositionRuleConfiguration>
                {
                    [CompositionRule.AvoidDissonance] = new(CompositionRule.AvoidDissonance, ConfigurationStatus.Enabled, 2),
                    [CompositionRule.AvoidRepeatedChords] = new(CompositionRule.AvoidRepeatedChords, ConfigurationStatus.Enabled, 3),
                    [CompositionRule.AvoidDirectFifths] = new(CompositionRule.AvoidDirectFifths, ConfigurationStatus.Disabled, 4)
                }
            )
        );

        // assert
        state.Configurations
            .Should()
            .ContainKeys(CompositionRule.AvoidDirectFifths, CompositionRule.AvoidDissonance, CompositionRule.AvoidRepeatedChords);

        state[CompositionRule.AvoidDirectFifths]!.Strictness.Should().Be(4);
        state[CompositionRule.AvoidDirectFifths]!.IsEnabled.Should().BeFalse();
        state[CompositionRule.AvoidDissonance]!.Strictness.Should().Be(2);
        state[CompositionRule.AvoidDissonance]!.IsEnabled.Should().BeTrue();
        state[CompositionRule.AvoidRepeatedChords]!.Strictness.Should().Be(3);
        state[CompositionRule.AvoidRepeatedChords]!.IsEnabled.Should().BeTrue();
    }
}
