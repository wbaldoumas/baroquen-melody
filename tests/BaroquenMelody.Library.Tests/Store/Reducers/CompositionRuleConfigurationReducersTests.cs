using BaroquenMelody.Library.Compositions.Rules.Enums;
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
        state = CompositionRuleConfigurationReducers.ReduceUpdateCompositionRuleConfiguration(state, new UpdateCompositionRuleConfiguration(CompositionRule.AvoidDirectFifths, true, 1));
        state = CompositionRuleConfigurationReducers.ReduceUpdateCompositionRuleConfiguration(state, new UpdateCompositionRuleConfiguration(CompositionRule.AvoidDissonance, true, 2));
        state = CompositionRuleConfigurationReducers.ReduceUpdateCompositionRuleConfiguration(state, new UpdateCompositionRuleConfiguration(CompositionRule.AvoidRepeatedChords, true, 3));
        state = CompositionRuleConfigurationReducers.ReduceUpdateCompositionRuleConfiguration(state, new UpdateCompositionRuleConfiguration(CompositionRule.AvoidDirectFifths, false, 4));

        // assert
        state.Configurations
            .Should()
            .ContainKeys(CompositionRule.AvoidDirectFifths, CompositionRule.AvoidDissonance, CompositionRule.AvoidRepeatedChords);

        state.Configurations[CompositionRule.AvoidDirectFifths].Strictness.Should().Be(4);
        state.Configurations[CompositionRule.AvoidDirectFifths].IsEnabled.Should().BeFalse();

        state.Configurations[CompositionRule.AvoidDissonance].Strictness.Should().Be(2);
        state.Configurations[CompositionRule.AvoidDissonance].IsEnabled.Should().BeTrue();

        state.Configurations[CompositionRule.AvoidRepeatedChords].Strictness.Should().Be(3);
        state.Configurations[CompositionRule.AvoidRepeatedChords].IsEnabled.Should().BeTrue();
    }
}
