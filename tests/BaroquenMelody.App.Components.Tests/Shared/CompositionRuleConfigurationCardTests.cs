using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.App.Components.Tests.TestData;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class CompositionRuleConfigurationCardTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Card_renders_the_rule_name()
    {
        // act
        var component = RenderCard(CompositionRule.AvoidDissonance);

        // assert
        component.Markup.Should().Contain("Avoid Dissonance");
    }

    [Test]
    public void Clicking_the_help_icon_opens_the_rule_description()
    {
        // arrange
        var component = RenderCard(CompositionRule.EnforceVoiceSpacing);

        // act
        component.FindAll("button")[0].Click();

        // assert
        _testContext.PopoverProvider.Markup.Should().Contain("Spacing keeps adjacent upper voices");
    }

    [Test]
    public void Disabling_the_rule_updates_the_store()
    {
        // arrange
        var component = RenderCard(CompositionRule.AvoidDissonance);

        // act
        component.EnableSwitch().Change(false);

        // assert
        RuleStatus(CompositionRule.AvoidDissonance).Should().Be(ConfigurationStatus.Disabled);
    }

    [Test]
    public void Changing_the_strictness_updates_the_store_and_keeps_the_status()
    {
        // arrange
        var component = RenderCard(CompositionRule.AvoidDissonance);

        // act
        component.Find("input[type=number]").Change("55");

        // assert
        var configuration = _testContext.StateOf<CompositionRuleConfigurationState>()[CompositionRule.AvoidDissonance]!;

        configuration.Strictness.Should().Be(55);
        configuration.Status.Should().Be(ConfigurationStatus.Enabled);
    }

    [Test]
    public void Strictness_input_is_disabled_for_a_frozen_rule()
    {
        // arrange
        var component = RenderCard(CompositionRule.AvoidDissonance);

        // act
        component.EnableSwitch().Change(false);

        // assert
        component.Find("input[type=number]").HasAttribute("disabled").Should().BeTrue();
    }

    [Test]
    public void Auto_disabled_chip_is_shown_for_the_enabled_spacing_rule_when_unsatisfiable()
    {
        // arrange
        var component = RenderCard(CompositionRule.EnforceVoiceSpacing);

        // act
        VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);
        component.Render();

        // assert
        component.Markup.Should().Contain("Auto-disabled");
    }

    [Test]
    public void Other_rules_never_show_the_auto_disabled_chip()
    {
        // arrange
        var component = RenderCard(CompositionRule.AvoidVoiceCrossing);

        // act
        VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);
        component.Render();

        // assert
        component.Markup.Should().NotContain("Auto-disabled");
    }

    [Test]
    public void Manual_disable_survives_an_unsatisfiable_range_round_trip()
    {
        // arrange: disable the spacing rule while the ranges are satisfiable
        var component = RenderCard(CompositionRule.EnforceVoiceSpacing);

        component.EnableSwitch().Change(false);

        // act: move the top voice out of the satisfiable range and back in
        var originalConfiguration = VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);

        component.Markup.Should().NotContain("Auto-disabled", "a manually disabled rule is not auto-disabled");

        VoiceSpacingScenarios.RestoreTopVoice(_testContext, originalConfiguration);

        // assert: the manual disable is preserved - the rule must not auto-enable
        RuleStatus(CompositionRule.EnforceVoiceSpacing).Should().Be(ConfigurationStatus.Disabled);
        component.EnableSwitch().IsChecked.Should().BeFalse();
    }

    [Test]
    public void Disabling_while_auto_disabled_lands_and_survives_restoring_the_ranges()
    {
        // arrange: the ranges are already unsatisfiable when the user reaches for the switch
        var originalConfiguration = VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);

        var component = RenderCard(CompositionRule.EnforceVoiceSpacing);

        // the switch keeps showing the stored status and stays interactive
        component.EnableSwitch().IsChecked.Should().BeTrue();
        component.EnableSwitch().IsDisabled.Should().BeFalse();

        // act: the disable click lands in the store, and the chip disappears with the rule off
        component.EnableSwitch().Change(false);

        RuleStatus(CompositionRule.EnforceVoiceSpacing).Should().Be(ConfigurationStatus.Disabled);
        component.Markup.Should().NotContain("Auto-disabled");

        VoiceSpacingScenarios.RestoreTopVoice(_testContext, originalConfiguration);

        // assert: restoring satisfiability must not re-arm the rule
        RuleStatus(CompositionRule.EnforceVoiceSpacing).Should().Be(ConfigurationStatus.Disabled);
        component.EnableSwitch().IsChecked.Should().BeFalse();
    }

    private ConfigurationStatus RuleStatus(CompositionRule rule) => _testContext.StateOf<CompositionRuleConfigurationState>()[rule]!.Status;

    private IRenderedComponent<CompositionRuleConfigurationCard> RenderCard(CompositionRule rule) => _testContext.RenderComponent<CompositionRuleConfigurationCard>(parameters => parameters
        .Add(component => component.CompositionRule, rule)
    );
}
