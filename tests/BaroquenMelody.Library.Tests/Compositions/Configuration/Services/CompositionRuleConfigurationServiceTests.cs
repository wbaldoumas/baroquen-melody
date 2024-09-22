using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Configurations.Services;
using BaroquenMelody.Library.Compositions.Rules.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Fluxor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Configuration.Services;

[TestFixture]
internal sealed class CompositionRuleConfigurationServiceTests
{
    private IDispatcher _mockDispatcher = null!;

    private IState<CompositionRuleConfigurationState> _mockState = null!;

    private CompositionRuleConfigurationService _compositionRuleConfigurationService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockState = Substitute.For<IState<CompositionRuleConfigurationState>>();

        _mockState.Value.Returns(new CompositionRuleConfigurationState());

        _compositionRuleConfigurationService = new CompositionRuleConfigurationService(_mockDispatcher, _mockState);
    }

    [Test]
    public void ConfigurableCompositionRules_returns_expected_values()
    {
        // arrange
        var expectedConfigurableCompositionRules = new[]
        {
            CompositionRule.AvoidDissonance,
            CompositionRule.AvoidDissonantLeaps,
            CompositionRule.HandleAscendingSeventh,
            CompositionRule.AvoidRepetition,
            CompositionRule.AvoidParallelFourths,
            CompositionRule.AvoidParallelFifths,
            CompositionRule.AvoidParallelOctaves,
            CompositionRule.AvoidDirectFourths,
            CompositionRule.AvoidDirectFifths,
            CompositionRule.AvoidDirectOctaves,
            CompositionRule.AvoidOverDoubling,
            CompositionRule.FollowStandardChordProgression,
            CompositionRule.AvoidRepeatedChords
        };

        // act
        var actualConfigurableCompositionRules = _compositionRuleConfigurationService.ConfigurableCompositionRules;

        // assert
        actualConfigurableCompositionRules.Should().BeEquivalentTo(expectedConfigurableCompositionRules);
    }

    [Test]
    public void ConfigureDefaults_dispatches_expected_actions()
    {
        // act
        _compositionRuleConfigurationService.ConfigureDefaults();

        // assert
        _mockDispatcher
            .Received(AggregateCompositionRuleConfiguration.Default.Configurations.Count)
            .Dispatch(Arg.Any<UpdateCompositionRuleConfiguration>());
    }

    [Test]
    public void Randomize_dispatches_expected_actions()
    {
        // act
        _compositionRuleConfigurationService.Randomize();

        // assert
        _mockDispatcher
            .Received(AggregateCompositionRuleConfiguration.Default.Configurations.Count)
            .Dispatch(Arg.Any<UpdateCompositionRuleConfiguration>());
    }
}
