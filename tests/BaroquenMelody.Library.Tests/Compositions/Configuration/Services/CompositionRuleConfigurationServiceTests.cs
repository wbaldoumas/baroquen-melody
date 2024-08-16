using BaroquenMelody.Library.Compositions.Configurations.Services;
using BaroquenMelody.Library.Compositions.Rules.Enums;
using BaroquenMelody.Library.Store.Actions;
using FluentAssertions;
using Fluxor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Configuration.Services;

[TestFixture]
internal sealed class CompositionRuleConfigurationServiceTests
{
    private IDispatcher _mockDispatcher = null!;

    private CompositionRuleConfigurationService _compositionRuleConfigurationService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();

        _compositionRuleConfigurationService = new CompositionRuleConfigurationService(_mockDispatcher);
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
        _mockDispatcher.Received(13).Dispatch(Arg.Any<UpdateCompositionRuleConfiguration>());
    }
}
