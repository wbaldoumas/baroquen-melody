using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class CompositionRuleConfigurationPanelTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Panel_renders_a_card_per_configurable_rule()
    {
        // arrange
        var configurableRuleCount = _testContext.Services.GetRequiredService<ICompositionRuleConfigurationService>().ConfigurableCompositionRules.Count();

        // act
        var component = _testContext.RenderComponent<CompositionRuleConfigurationPanel>();

        // assert
        component.FindComponents<CompositionRuleConfigurationCard>().Should().HaveCount(configurableRuleCount);
    }

    [Test]
    public void Search_filters_the_rule_cards()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionRuleConfigurationPanel>();

        // act
        component.Find("input[type=text]").Input("Voice");

        // assert: avoid voice crossing, avoid voice overlap, and enforce voice spacing match
        component.FindComponents<CompositionRuleConfigurationCard>().Should().HaveCount(3);
    }

    [Test]
    public void Search_without_matches_shows_an_alert()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionRuleConfigurationPanel>();

        // act
        component.Find("input[type=text]").Input("no such rule");

        // assert
        component.FindComponents<CompositionRuleConfigurationCard>().Should().BeEmpty();
        component.Markup.Should().Contain("No composition rules found");
    }

    [Test]
    public void Reset_restores_the_default_rule_configurations()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionRuleConfigurationPanel>();
        var strictness = _testContext.StateOf<CompositionRuleConfigurationState>()[CompositionRule.EnforceVoiceSpacing]!.Strictness;

        _testContext.Dispatcher.Dispatch(new UpdateCompositionRuleConfiguration(CompositionRule.EnforceVoiceSpacing, ConfigurationStatus.Disabled, strictness));

        // act: re-render first - the dispatch re-rendered the cards, and clicking through a stale
        // markup snapshot silently no-ops in bUnit
        component.Render();
        component.FindAll("button").First(button => button.TextContent.Contains("Reset", StringComparison.Ordinal)).Click();

        // assert
        _testContext.StateOf<CompositionRuleConfigurationState>()[CompositionRule.EnforceVoiceSpacing]!.Status.Should().Be(ConfigurationStatus.Enabled);
    }
}
