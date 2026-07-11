using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Melanchall.DryWetMidi.Standards;
using MudBlazor;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class MidiInstrumentAutocompleteSelectorTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Displays_the_stored_midi_instrument()
    {
        // act
        var component = RenderSelector();

        // assert
        component.Find("input").GetAttribute("value").Should().Be("Acoustic Grand Piano");
    }

    [Test]
    public void Choosing_an_instrument_updates_the_store()
    {
        // arrange
        var component = RenderSelector();
        var autocomplete = component.FindComponent<MudAutocomplete<GeneralMidi2Program>>();

        // act
        component.InvokeAsync(() => autocomplete.Instance.ValueChanged.InvokeAsync(GeneralMidi2Program.Violin)).GetAwaiter().GetResult();

        // assert
        _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!.MidiProgram.Should().Be(GeneralMidi2Program.Violin);
    }

    [Test]
    public void Search_filters_instruments_by_name()
    {
        // arrange
        var component = RenderSelector();
        var autocomplete = component.FindComponent<MudAutocomplete<GeneralMidi2Program>>();

        // act
        var searchResults = autocomplete.Instance.SearchFunc!("violin", CancellationToken.None)!.GetAwaiter().GetResult()!.ToList();

        // assert
        searchResults.Should().Contain(GeneralMidi2Program.Violin);
        searchResults.Should().NotContain(GeneralMidi2Program.AcousticGrandPiano);
    }

    [Test]
    public void Empty_search_returns_the_full_catalog_in_alphabetical_order()
    {
        // arrange
        var component = RenderSelector();
        var autocomplete = component.FindComponent<MudAutocomplete<GeneralMidi2Program>>();

        // act
        var searchResults = autocomplete.Instance.SearchFunc!(string.Empty, CancellationToken.None)!.GetAwaiter().GetResult()!.ToList();

        // assert
        searchResults.Should().Contain(GeneralMidi2Program.AcousticGrandPiano);
        searchResults.Should().Contain(GeneralMidi2Program.Violin);
    }

    [Test]
    public void Filtering_out_the_current_instrument_type_switches_to_an_available_instrument()
    {
        // arrange
        var component = RenderSelector();

        component.Find("div.mud-input-adornment button").Click();

        // act: the first filter type covers the pianos, so unchecking it excludes the stored acoustic grand piano
        _testContext.PopoverProvider.FindAll("input[type=checkbox]")[0].Change(false);

        // assert
        _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!.MidiProgram.Should().NotBe(GeneralMidi2Program.AcousticGrandPiano);
    }

    [Test]
    public void Disabled_selector_disables_the_autocomplete_input()
    {
        // act
        var component = RenderSelector(ConfigurationStatus.DisabledAndLocked);

        // assert
        component.Find("input").HasAttribute("disabled").Should().BeTrue();
    }

    private IRenderedComponent<MidiInstrumentAutocompleteSelector> RenderSelector(ConfigurationStatus status = ConfigurationStatus.Enabled) => _testContext.RenderComponent<MidiInstrumentAutocompleteSelector>(parameters => parameters
        .Add(component => component.Instrument, Instrument.One)
        .Add(component => component.Status, status)
    );
}
