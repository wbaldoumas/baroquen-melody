using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.App.Components.Tests.TestComponents;
using BaroquenMelody.App.Components.Tests.TestData;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class InstrumentConfigurationPanelTests
{
    private AppComponentsTestContext _testContext = null!;

    private ISnackbar Snackbar => _testContext.Services.GetRequiredService<ISnackbar>();

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Panel_renders_a_card_per_configurable_instrument()
    {
        // act
        var component = _testContext.RenderComponent<InstrumentConfigurationPanel>();

        // assert
        component.FindComponents<InstrumentConfigurationCard>().Should().HaveCount(4);
    }

    [Test]
    public void Disabling_every_instrument_shows_the_invalid_instrumentation_alert()
    {
        // arrange
        var component = _testContext.RenderComponent<InstrumentConfigurationPanel>();

        // act: instrument four is disabled by default
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentConfigurationStatus(Instrument.One, ConfigurationStatus.Disabled));
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentConfigurationStatus(Instrument.Two, ConfigurationStatus.Disabled));
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentConfigurationStatus(Instrument.Three, ConfigurationStatus.Disabled));
        component.Render();

        // assert
        component.Markup.Should().Contain("Invalid instrumentation");
    }

    [Test]
    public void Reset_restores_the_default_instrument_configurations()
    {
        // arrange
        var component = _testContext.RenderComponent<InstrumentConfigurationPanel>();

        VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);

        // act: re-render first - the dispatch re-rendered the cards, and clicking through a stale
        // markup snapshot silently no-ops in bUnit
        component.Render();
        component.FindAll("button").First(button => button.TextContent.Contains("Reset", StringComparison.Ordinal)).Click();

        // assert
        _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!.MinNote.Should().Be(Notes.C5);
    }

    [Test]
    public void Making_the_enabled_spacing_rule_unsatisfiable_shows_a_warning_toast()
    {
        // arrange: the voice spacing rule is enabled by default
        _testContext.RenderComponent<InstrumentConfigurationPanel>();

        // act
        VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);

        // assert
        Snackbar.ShownSnackbars.Should().ContainSingle();
    }

    [Test]
    public void No_toast_when_the_spacing_rule_is_manually_disabled()
    {
        // arrange
        var strictness = _testContext.StateOf<CompositionRuleConfigurationState>()[CompositionRule.EnforceVoiceSpacing]!.Strictness;

        _testContext.Dispatcher.Dispatch(new UpdateCompositionRuleConfiguration(CompositionRule.EnforceVoiceSpacing, ConfigurationStatus.Disabled, strictness));
        _testContext.RenderComponent<InstrumentConfigurationPanel>();

        // act
        VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);

        // assert
        Snackbar.ShownSnackbars.Should().BeEmpty();
    }

    [Test]
    public void No_toast_when_the_panel_mounts_with_an_already_unsatisfiable_configuration()
    {
        // arrange
        var originalConfiguration = VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);

        // act
        _testContext.RenderComponent<InstrumentConfigurationPanel>();

        // assert: no toast on mount, and none when the configuration becomes satisfiable again
        Snackbar.ShownSnackbars.Should().BeEmpty();

        VoiceSpacingScenarios.RestoreTopVoice(_testContext, originalConfiguration);

        Snackbar.ShownSnackbars.Should().BeEmpty();
    }

    [Test]
    public void Each_unsatisfiable_transition_toasts_again()
    {
        // arrange
        _testContext.RenderComponent<InstrumentConfigurationPanel>();

        var originalConfiguration = VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);

        Snackbar.ShownSnackbars.Should().ContainSingle();

        // act: dismiss the first toast, bring the voice back in, and move it out again
        Snackbar.Clear();
        VoiceSpacingScenarios.RestoreTopVoice(_testContext, originalConfiguration);
        VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);

        // assert
        Snackbar.ShownSnackbars.Should().ContainSingle();
    }

    [Test]
    public void Unmounted_panel_no_longer_toasts()
    {
        // arrange: mount and unmount the panel, as a tab switch does
        var component = _testContext.RenderComponent<ConditionalWrapper>(parameters => parameters
            .Add(wrapper => wrapper.Show, true)
            .AddChildContent<InstrumentConfigurationPanel>()
        );

        component.SetParametersAndRender(parameters => parameters.Add(wrapper => wrapper.Show, false));

        // act
        VoiceSpacingScenarios.MoveTopVoiceOutOfSatisfiableRange(_testContext);

        // assert: a leaked state subscription from the unmounted panel would still toast
        Snackbar.ShownSnackbars.Should().BeEmpty();
    }
}
