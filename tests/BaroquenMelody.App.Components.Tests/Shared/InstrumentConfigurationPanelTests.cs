using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.App.Components.Tests.TestComponents;
using BaroquenMelody.App.Components.Tests.TestData;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms.Enums;
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

        // act
        component.ClickButtonByText("Reset");

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
    public void Emptying_the_ground_bank_while_in_ground_bass_form_toasts()
    {
        // arrange: the range edit leaves voice spacing satisfiable, so the single toast is the ground warning.
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        _testContext.RenderComponent<InstrumentConfigurationPanel>();

        // act
        GroundBassScenarios.EmptyTheGroundBank(_testContext);

        // assert
        Snackbar.ShownSnackbars.Should().ContainSingle();
    }

    [Test]
    public void No_ground_toast_for_a_range_edit_that_merely_shrinks_the_bank()
    {
        // arrange
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        _testContext.RenderComponent<InstrumentConfigurationPanel>();

        // act: only the tetrachord survives, but the composition still grounds - the card's chip suffices.
        GroundBassScenarios.ReduceTheGroundBankToTheTetrachord(_testContext);

        // assert
        Snackbar.ShownSnackbars.Should().BeEmpty();
    }

    [Test]
    public void A_range_edit_that_breaks_the_pinned_pattern_toasts_even_though_the_bank_is_not_empty()
    {
        // arrange: the user pinned the romanesca; the shrink leaves the tetrachord feasible, so the bank
        // is not empty - but the pinned selection is falling back to the fugue, which deserves the toast.
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        GroundBassScenarios.SelectGroundBassPattern(_testContext, GroundBass.Romanesca);
        _testContext.RenderComponent<InstrumentConfigurationPanel>();

        // act
        GroundBassScenarios.ReduceTheGroundBankToTheTetrachord(_testContext);

        // assert
        Snackbar.ShownSnackbars.Should().ContainSingle();
    }

    [Test]
    public void No_ground_toast_when_the_fugue_form_is_selected()
    {
        // arrange
        _testContext.RenderComponent<InstrumentConfigurationPanel>();

        // act: an empty bank is irrelevant to a fugue composition
        GroundBassScenarios.EmptyTheGroundBank(_testContext);

        // assert
        Snackbar.ShownSnackbars.Should().BeEmpty();
    }

    [Test]
    public void No_ground_toast_when_the_panel_mounts_with_an_already_empty_bank()
    {
        // arrange
        GroundBassScenarios.SelectGroundBassForm(_testContext);

        var originalConfiguration = GroundBassScenarios.EmptyTheGroundBank(_testContext);

        // act
        _testContext.RenderComponent<InstrumentConfigurationPanel>();

        // assert: no toast on mount, and none when the bank becomes feasible again
        Snackbar.ShownSnackbars.Should().BeEmpty();

        GroundBassScenarios.RestoreGroundHostingVoice(_testContext, originalConfiguration);

        Snackbar.ShownSnackbars.Should().BeEmpty();
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
