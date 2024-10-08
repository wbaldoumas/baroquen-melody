﻿using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Fluxor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configuration.Services;

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
            CompositionRule.AvoidRepeatedNotes,
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
            .Received(1)
            .Dispatch(Arg.Any<BatchUpdateCompositionRuleConfiguration>());
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
            { CompositionRule.AvoidRepeatedNotes, new CompositionRuleConfiguration(CompositionRule.AvoidRepeatedNotes) },
            { CompositionRule.FollowStandardChordProgression, new CompositionRuleConfiguration(CompositionRule.FollowStandardChordProgression) },
            { CompositionRule.HandleAscendingSeventh, new CompositionRuleConfiguration(CompositionRule.HandleAscendingSeventh, Status: ConfigurationStatus.Locked) },
            { CompositionRule.AvoidRepeatedChords, new CompositionRuleConfiguration(CompositionRule.AvoidRepeatedChords, Status: ConfigurationStatus.Disabled) }
        };

        _mockState.Value.Returns(new CompositionRuleConfigurationState(configurations));

        // act
        _compositionRuleConfigurationService.Randomize();

        // assert
        _mockDispatcher
            .Received(1)
            .Dispatch(Arg.Any<BatchUpdateCompositionRuleConfiguration>());
    }
}
