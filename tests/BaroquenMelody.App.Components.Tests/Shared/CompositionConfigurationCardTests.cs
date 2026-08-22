using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.App.Components.Tests.TestComponents;
using BaroquenMelody.App.Components.Tests.TestData;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class CompositionConfigurationCardTests
{
    private AppComponentsTestContext _testContext = null!;

    private ISnackbar Snackbar => _testContext.Services.GetRequiredService<ISnackbar>();

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Card_renders_the_default_composition_configuration()
    {
        // act
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        // assert: C ionian in four-four time, 25 measures minimum, at 120 beats per minute
        var numericInputs = component.FindAll("input[type=number]");

        numericInputs[0].GetAttribute("value").Should().Be("25");
        numericInputs[1].GetAttribute("value").Should().Be("120");
    }

    [Test]
    public void Choosing_a_tonic_note_updates_the_store()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var select = component.FindComponent<MudSelect<NoteName>>();

        // act
        component.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(NoteName.G)).GetAwaiter().GetResult();

        // assert
        _testContext.StateOf<CompositionConfigurationState>().TonicNote.Should().Be(NoteName.G);
    }

    [Test]
    public void Choosing_a_mode_updates_the_store()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var select = component.FindComponent<MudSelect<Mode>>();

        // act
        component.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(Mode.Dorian)).GetAwaiter().GetResult();

        // assert
        _testContext.StateOf<CompositionConfigurationState>().Mode.Should().Be(Mode.Dorian);
    }

    [Test]
    public void Choosing_a_meter_updates_the_store()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var select = component.FindComponent<MudSelect<Meter>>();

        // act
        component.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(Meter.ThreeFour)).GetAwaiter().GetResult();

        // assert
        _testContext.StateOf<CompositionConfigurationState>().Meter.Should().Be(Meter.ThreeFour);
    }

    [Test]
    public void Changing_the_minimum_measures_updates_the_store()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        // act
        component.FindAll("input[type=number]")[0].Change("50");

        // assert
        _testContext.StateOf<CompositionConfigurationState>().MinimumMeasures.Should().Be(50);
    }

    [Test]
    public void Changing_the_tempo_updates_the_store()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        // act
        component.FindAll("input[type=number]")[1].Change("90");

        // assert
        _testContext.StateOf<CompositionConfigurationState>().Tempo.Should().Be(90);
    }

    [Test]
    public void Choosing_the_ground_bass_form_updates_the_store()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var select = component.FindComponent<MudSelect<CompositionForm>>();

        // act
        component.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(CompositionForm.GroundBass)).GetAwaiter().GetResult();

        // assert
        _testContext.StateOf<CompositionConfigurationState>().Form.Should().Be(CompositionForm.GroundBass);
    }

    [Test]
    public void Changing_the_tonic_preserves_the_selected_form()
    {
        // arrange: every handler must carry the whole state forward, or a tonic change would silently
        // reset the form to the fugue default.
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var formSelect = component.FindComponent<MudSelect<CompositionForm>>();

        component.InvokeAsync(() => formSelect.Instance.ValueChanged.InvokeAsync(CompositionForm.GroundBass)).GetAwaiter().GetResult();
        component.Render();

        var tonicSelect = component.FindComponent<MudSelect<NoteName>>();

        // act
        component.InvokeAsync(() => tonicSelect.Instance.ValueChanged.InvokeAsync(NoteName.G)).GetAwaiter().GetResult();

        // assert
        var state = _testContext.StateOf<CompositionConfigurationState>();

        state.TonicNote.Should().Be(NoteName.G);
        state.Form.Should().Be(CompositionForm.GroundBass);
    }

    [Test]
    public void The_pattern_dropdown_appears_only_under_the_ground_bass_form()
    {
        // arrange: the pattern is meaningless to a fugue, so the fugue form hides the dropdown entirely.
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        component.FindComponents<SelectWithPopover<GroundBass?>>().Should().BeEmpty();

        // act
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        component.Render();

        // assert
        component.FindComponents<SelectWithPopover<GroundBass?>>().Should().ContainSingle();
    }

    [Test]
    public void The_texture_dropdown_appears_only_under_the_fugue_form()
    {
        // arrange: the texture shapes the fugal body's accompaniment, so the ground bass form (whose
        // texture is its divisions) hides the dropdown entirely.
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        component.FindComponents<SelectWithPopover<TextureType>>().Should().ContainSingle();

        // act
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        component.Render();

        // assert
        component.FindComponents<SelectWithPopover<TextureType>>().Should().BeEmpty();
    }

    [Test]
    public void Choosing_a_texture_updates_the_store()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var select = component.FindComponent<MudSelect<TextureType>>();

        // act
        component.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(TextureType.Walking)).GetAwaiter().GetResult();

        // assert
        _testContext.StateOf<CompositionConfigurationState>().Texture.Should().Be(TextureType.Walking);
    }

    [Test]
    public void Changing_the_tonic_preserves_the_selected_texture()
    {
        // arrange: every handler must carry the whole state forward, or a tonic change would silently
        // reset the texture to the imitative default.
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var textureSelect = component.FindComponent<MudSelect<TextureType>>();

        component.InvokeAsync(() => textureSelect.Instance.ValueChanged.InvokeAsync(TextureType.BrokenChord)).GetAwaiter().GetResult();
        component.Render();

        var tonicSelect = component.FindComponent<MudSelect<NoteName>>();

        // act
        component.InvokeAsync(() => tonicSelect.Instance.ValueChanged.InvokeAsync(NoteName.G)).GetAwaiter().GetResult();

        // assert
        var state = _testContext.StateOf<CompositionConfigurationState>();

        state.TonicNote.Should().Be(NoteName.G);
        state.Texture.Should().Be(TextureType.BrokenChord);
    }

    [Test]
    public void Choosing_a_ground_bass_pattern_updates_the_store()
    {
        // arrange
        GroundBassScenarios.SelectGroundBassForm(_testContext);

        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var select = component.FindComponent<MudSelect<GroundBass?>>();

        // act
        component.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(GroundBass.Romanesca)).GetAwaiter().GetResult();

        // assert
        _testContext.StateOf<CompositionConfigurationState>().GroundBassPattern.Should().Be(GroundBass.Romanesca);
    }

    [Test]
    public void Changing_the_tonic_preserves_the_selected_pattern()
    {
        // arrange: every handler must carry the whole state forward, or a tonic change would silently
        // reset the pinned pattern to the free draw.
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        GroundBassScenarios.SelectGroundBassPattern(_testContext, GroundBass.CadentialGround);

        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var tonicSelect = component.FindComponent<MudSelect<NoteName>>();

        // act
        component.InvokeAsync(() => tonicSelect.Instance.ValueChanged.InvokeAsync(NoteName.G)).GetAwaiter().GetResult();

        // assert
        var state = _testContext.StateOf<CompositionConfigurationState>();

        state.TonicNote.Should().Be(NoteName.G);
        state.GroundBassPattern.Should().Be(GroundBass.CadentialGround);
    }

    [Test]
    public void The_modulate_switch_appears_only_under_the_ground_bass_form()
    {
        // arrange: modulation is a ground bass journey, so the fugue form hides the switch entirely.
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        component.FindComponents<MudSwitch<bool>>().Should().BeEmpty();

        // act
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        component.Render();

        // assert
        component.FindComponents<MudSwitch<bool>>().Should().ContainSingle();
    }

    [Test]
    public void Toggling_modulation_updates_the_store()
    {
        // arrange
        GroundBassScenarios.SelectGroundBassForm(_testContext);

        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var modulateSwitch = component.FindComponent<MudSwitch<bool>>();

        // act
        component.InvokeAsync(() => modulateSwitch.Instance.ValueChanged.InvokeAsync(false)).GetAwaiter().GetResult();

        // assert
        _testContext.StateOf<CompositionConfigurationState>().GroundBassModulate.Should().BeFalse();
    }

    [Test]
    public void Changing_the_tonic_preserves_the_modulation_toggle()
    {
        // arrange: every handler must carry the whole state forward, or a tonic change would silently
        // restore the default journey the user turned off.
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        GroundBassScenarios.SetGroundBassModulate(_testContext, modulate: false);

        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var tonicSelect = component.FindComponent<MudSelect<NoteName>>();

        // act
        component.InvokeAsync(() => tonicSelect.Instance.ValueChanged.InvokeAsync(NoteName.G)).GetAwaiter().GetResult();

        // assert
        var state = _testContext.StateOf<CompositionConfigurationState>();

        state.TonicNote.Should().Be(NoteName.G);
        state.GroundBassModulate.Should().BeFalse();
    }

    [Test]
    public void The_pattern_dropdown_marks_patterns_that_do_not_fit()
    {
        // arrange: a G3-B4 ground-hosting voice fits only the tetrachord for a C tonic, so the dropdown
        // must say so on the infeasible entries and label the free draw as Random.
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        GroundBassScenarios.ReduceTheGroundBankToTheTetrachord(_testContext);

        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var select = component.FindComponent<SelectWithPopover<GroundBass?>>();

        // act + assert
        select.Instance.ConvertToDisplay(null).Should().Be("Random");
        select.Instance.ConvertToDisplay(GroundBass.DescendingTetrachord).Should().Be("Descending Tetrachord");
        select.Instance.ConvertToDisplay(GroundBass.Romanesca).Should().Be("Romanesca (doesn't fit)");
        select.Instance.ConvertToDisplay(GroundBass.CadentialGround).Should().Be("Cadential Ground (doesn't fit)");
    }

    [Test]
    public void A_pinned_pattern_that_does_not_fit_shows_the_selected_ground_warning_chip()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        GroundBassScenarios.SelectGroundBassForm(_testContext);
        GroundBassScenarios.ReduceTheGroundBankToTheTetrachord(_testContext);

        // act
        GroundBassScenarios.SelectGroundBassPattern(_testContext, GroundBass.Romanesca);
        component.Render();

        // assert
        component.Markup.Should().Contain("Selected ground doesn't fit");
    }

    [Test]
    public void A_pinned_pattern_that_fits_needs_no_chip_even_when_the_bank_is_reduced()
    {
        // arrange: the count chip informs the free draw's variety; a satisfied pinned selection has
        // nothing to warn about.
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        GroundBassScenarios.SelectGroundBassForm(_testContext);
        GroundBassScenarios.ReduceTheGroundBankToTheTetrachord(_testContext);

        // act
        GroundBassScenarios.SelectGroundBassPattern(_testContext, GroundBass.DescendingTetrachord);
        component.Render();

        // assert
        component.Markup.Should().NotContain("grounds fit").And.NotContain("doesn't fit");
    }

    [Test]
    public void Pinning_a_pattern_that_does_not_fit_toasts_the_selected_pattern_fallback()
    {
        // arrange: the bank still hosts the tetrachord, so only the pinned selection is falling back.
        _testContext.RenderComponent<CompositionConfigurationCard>();

        GroundBassScenarios.SelectGroundBassForm(_testContext);
        GroundBassScenarios.ReduceTheGroundBankToTheTetrachord(_testContext);

        Snackbar.ShownSnackbars.Should().BeEmpty();

        // act
        GroundBassScenarios.SelectGroundBassPattern(_testContext, GroundBass.Romanesca);

        // assert
        Snackbar.ShownSnackbars.Should().ContainSingle();
    }

    [Test]
    public void No_feasibility_chip_when_every_ground_fits()
    {
        // arrange: the default ranges host the whole bank, so the ground bass form needs no caveat.
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        // act
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        component.Render();

        // assert
        component.Markup.Should().NotContain("grounds fit");
    }

    [Test]
    public void No_feasibility_chip_when_the_fugue_form_is_selected()
    {
        // arrange: an empty bank is irrelevant to a fugue, so the chip stays hidden.
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        // act
        GroundBassScenarios.EmptyTheGroundBank(_testContext);
        component.Render();

        // assert
        component.Markup.Should().NotContain("grounds fit");
    }

    [Test]
    public void A_reduced_bank_shows_a_persistent_count_chip_without_toasting()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        GroundBassScenarios.SelectGroundBassForm(_testContext);

        // act
        GroundBassScenarios.ReduceTheGroundBankToTheTetrachord(_testContext);
        component.Render();

        // assert: a shrinking-but-nonempty bank informs quietly - the chip appears, no toast fires.
        component.Markup.Should().Contain("1 of 3 grounds fits");
        Snackbar.ShownSnackbars.Should().BeEmpty();
    }

    [Test]
    public void An_empty_bank_shows_the_fugue_fallback_warning_chip()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();

        GroundBassScenarios.EmptyTheGroundBank(_testContext);

        // act
        GroundBassScenarios.SelectGroundBassForm(_testContext);
        component.Render();

        // assert
        component.Markup.Should().Contain("No grounds fit");
    }

    [Test]
    public void Selecting_the_ground_bass_form_over_an_empty_bank_toasts_the_fugue_fallback()
    {
        // arrange: the bank empties while the fugue form is selected, which is not yet worth a warning.
        _testContext.RenderComponent<CompositionConfigurationCard>();

        GroundBassScenarios.EmptyTheGroundBank(_testContext);

        Snackbar.ShownSnackbars.Should().BeEmpty();

        // act: choosing the ground bass form is the transition into the fallback.
        GroundBassScenarios.SelectGroundBassForm(_testContext);

        // assert
        Snackbar.ShownSnackbars.Should().ContainSingle();
    }

    [Test]
    public void Selecting_the_ground_bass_form_over_a_full_bank_does_not_toast()
    {
        // arrange
        _testContext.RenderComponent<CompositionConfigurationCard>();

        // act
        GroundBassScenarios.SelectGroundBassForm(_testContext);

        // assert
        Snackbar.ShownSnackbars.Should().BeEmpty();
    }

    [Test]
    public void Emptying_the_bank_by_key_change_while_in_ground_bass_form_toasts()
    {
        // arrange: a B tonic keeps a B2-B3 ground-hosting voice feasible (the tonic B3 tops the range with
        // a fourth below it), so selecting the ground bass form warns of nothing yet.
        _testContext.RenderComponent<CompositionConfigurationCard>();

        GroundBassScenarios.ChangeTonic(_testContext, NoteName.B);
        GroundBassScenarios.EmptyTheGroundBank(_testContext);
        GroundBassScenarios.SelectGroundBassForm(_testContext);

        Snackbar.ShownSnackbars.Should().BeEmpty();

        // act: in C the same range holds a single tonic with one scale step below it - the bank empties.
        GroundBassScenarios.ChangeTonic(_testContext, NoteName.C);

        // assert
        Snackbar.ShownSnackbars.Should().ContainSingle();
    }

    [Test]
    public void No_toast_when_the_card_mounts_with_an_already_fallen_back_configuration()
    {
        // arrange
        var originalConfiguration = GroundBassScenarios.EmptyTheGroundBank(_testContext);

        GroundBassScenarios.SelectGroundBassForm(_testContext);

        // act
        _testContext.RenderComponent<CompositionConfigurationCard>();

        // assert: no toast on mount, and none when the bank becomes feasible again
        Snackbar.ShownSnackbars.Should().BeEmpty();

        GroundBassScenarios.RestoreGroundHostingVoice(_testContext, originalConfiguration);

        Snackbar.ShownSnackbars.Should().BeEmpty();
    }

    [Test]
    public void Unmounted_card_no_longer_toasts()
    {
        // arrange: mount and unmount the card, as a tab switch does
        var component = _testContext.RenderComponent<ConditionalWrapper>(parameters => parameters
            .Add(wrapper => wrapper.Show, true)
            .AddChildContent<CompositionConfigurationCard>()
        );

        component.SetParametersAndRender(parameters => parameters.Add(wrapper => wrapper.Show, false));

        // act
        GroundBassScenarios.EmptyTheGroundBank(_testContext);
        GroundBassScenarios.SelectGroundBassForm(_testContext);

        // assert: a leaked state subscription from the unmounted card would still toast
        Snackbar.ShownSnackbars.Should().BeEmpty();
    }

    [Test]
    public void Changing_the_tonic_realigns_instrument_ranges_to_the_new_scale()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionConfigurationCard>();
        var select = component.FindComponent<MudSelect<NoteName>>();

        // act: the configuration effect snaps instrument ranges to the closest notes of the new scale
        component.InvokeAsync(() => select.Instance.ValueChanged.InvokeAsync(NoteName.FSharp)).GetAwaiter().GetResult();

        // assert
        var scale = _testContext.StateOf<CompositionConfigurationState>().Scale;
        var configuration = _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!;

        scale.GetNotes().Should().Contain(configuration.MinNote);
        scale.GetNotes().Should().Contain(configuration.MaxNote);
    }
}
