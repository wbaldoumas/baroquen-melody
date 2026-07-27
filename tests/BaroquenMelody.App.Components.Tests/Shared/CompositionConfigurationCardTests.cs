using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using MudBlazor;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class CompositionConfigurationCardTests
{
    private AppComponentsTestContext _testContext = null!;

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
