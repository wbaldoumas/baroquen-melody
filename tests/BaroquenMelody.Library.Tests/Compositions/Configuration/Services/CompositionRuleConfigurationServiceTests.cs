using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Configurations.Enums;
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
        // arrange
        var configurations = new Dictionary<CompositionRule, CompositionRuleConfiguration>
        {
            { CompositionRule.AvoidDirectFifths, new CompositionRuleConfiguration(CompositionRule.AvoidDirectFifths) },
            { CompositionRule.AvoidDirectFourths, new CompositionRuleConfiguration(CompositionRule.AvoidDirectFourths) },
            { CompositionRule.AvoidDirectOctaves, new CompositionRuleConfiguration(CompositionRule.AvoidDirectOctaves) },
            { CompositionRule.AvoidDissonance, new CompositionRuleConfiguration(CompositionRule.AvoidDissonance) },
            { CompositionRule.AvoidDissonantLeaps, new CompositionRuleConfiguration(CompositionRule.AvoidDissonantLeaps) },
            { CompositionRule.AvoidOverDoubling, new CompositionRuleConfiguration(CompositionRule.AvoidOverDoubling) },
            { CompositionRule.AvoidParallelFourths, new CompositionRuleConfiguration(CompositionRule.AvoidParallelFourths) },
            { CompositionRule.AvoidParallelFifths, new CompositionRuleConfiguration(CompositionRule.AvoidParallelFifths) },
            { CompositionRule.AvoidParallelOctaves, new CompositionRuleConfiguration(CompositionRule.AvoidParallelOctaves) },
            { CompositionRule.AvoidRepetition, new CompositionRuleConfiguration(CompositionRule.AvoidRepetition) },
            { CompositionRule.FollowStandardChordProgression, new CompositionRuleConfiguration(CompositionRule.FollowStandardChordProgression) },
            { CompositionRule.HandleAscendingSeventh, new CompositionRuleConfiguration(CompositionRule.HandleAscendingSeventh, Status: ConfigurationStatus.Locked) },
            { CompositionRule.AvoidRepeatedChords, new CompositionRuleConfiguration(CompositionRule.AvoidRepeatedChords, Status: ConfigurationStatus.Disabled) }
        };

        _mockState.Value.Returns(new CompositionRuleConfigurationState(configurations));

        // act
        _compositionRuleConfigurationService.Randomize();

        // assert
        _mockDispatcher
            .Received(configurations.Count - 2)
            .Dispatch(Arg.Any<UpdateCompositionRuleConfiguration>());
    }
}
