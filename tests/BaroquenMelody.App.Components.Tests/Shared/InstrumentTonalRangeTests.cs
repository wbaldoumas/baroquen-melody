using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using System.Globalization;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class InstrumentTonalRangeTests
{
    private AppComponentsTestContext _testContext = null!;

    private IList<Note> _availableNotes = null!;

    [SetUp]
    public void SetUp()
    {
        _testContext = new AppComponentsTestContext();
        _availableNotes = _testContext.StateOf<CompositionConfigurationState>().AvailableNotes;
    }

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Slider_labels_show_the_stored_tonal_range()
    {
        // act: instrument one defaults to C5 through E6
        var component = RenderTonalRange();

        // assert
        component.Markup.Should().ContainAll("C5", "E6");
    }

    [Test]
    public void Sliding_the_lower_thumb_updates_the_stored_minimum_note()
    {
        // arrange
        var component = RenderTonalRange();
        var newLowestPitchIndex = _availableNotes.IndexOf(Notes.C5) + 1;

        // act: the upper slider input renders first, the lower second
        component.FindAll("input.mud-slider-input")[1].Input(newLowestPitchIndex.ToString(CultureInfo.InvariantCulture));

        // assert
        var configuration = _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!;

        configuration.MinNote.Should().Be(_availableNotes[newLowestPitchIndex]);
        configuration.MaxNote.Should().Be(Notes.E6);
    }

    [Test]
    public void Sliding_the_upper_thumb_updates_the_stored_maximum_note()
    {
        // arrange
        var component = RenderTonalRange();
        var newHighestPitchIndex = _availableNotes.IndexOf(Notes.E6) - 1;

        // act
        component.FindAll("input.mud-slider-input")[0].Input(newHighestPitchIndex.ToString(CultureInfo.InvariantCulture));

        // assert
        var configuration = _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!;

        configuration.MinNote.Should().Be(Notes.C5);
        configuration.MaxNote.Should().Be(_availableNotes[newHighestPitchIndex]);
    }

    [Test]
    public void Widening_the_range_past_the_maximum_pulls_the_upper_note_along()
    {
        // arrange
        var component = RenderTonalRange();
        var newLowestPitchIndex = _availableNotes.IndexOf(Notes.E6) - CompositionConfiguration.MaxInstrumentRange - 1;

        // act
        component.FindAll("input.mud-slider-input")[1].Input(newLowestPitchIndex.ToString(CultureInfo.InvariantCulture));

        // assert
        var configuration = _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!;

        configuration.MinNote.Should().Be(_availableNotes[newLowestPitchIndex]);
        configuration.MaxNote.Should().Be(_availableNotes[newLowestPitchIndex + CompositionConfiguration.MaxInstrumentRange]);
    }

    [Test]
    public void Narrowing_the_range_past_the_minimum_pushes_the_upper_note_away()
    {
        // arrange
        var component = RenderTonalRange();
        var newLowestPitchIndex = _availableNotes.IndexOf(Notes.E6) - 3;

        // act
        component.FindAll("input.mud-slider-input")[1].Input(newLowestPitchIndex.ToString(CultureInfo.InvariantCulture));

        // assert
        var configuration = _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!;

        configuration.MinNote.Should().Be(_availableNotes[newLowestPitchIndex]);
        configuration.MaxNote.Should().Be(_availableNotes[newLowestPitchIndex + CompositionConfiguration.MinInstrumentRange]);
    }

    [Test]
    public void External_state_changes_are_reflected_in_the_rendered_range()
    {
        // arrange
        var component = RenderTonalRange();

        // act
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentTonalRange(Instrument.One, Notes.D5, Notes.D6));
        component.Render();

        // assert
        component.Markup.Should().ContainAll("D5", "D6");
    }

    [Test]
    public void Pitch_select_adornments_play_the_boundary_notes()
    {
        // arrange
        var component = RenderTonalRange();

        // act: each pitch select's adornment plays its boundary note
        var adornmentButtons = component.FindAll("div.mud-input-adornment button");

        adornmentButtons[0].Click();
        adornmentButtons[1].Click();

        // assert
        _testContext.JSInterop.Invocations.Count(invocation => invocation.Identifier == "startMidiPlayer").Should().Be(2);
    }

    [Test]
    public void Clicking_the_help_icon_opens_the_popover()
    {
        // arrange
        var component = RenderTonalRange();

        // act
        component.Find("button").Click();

        // assert
        _testContext.PopoverProvider.Markup.Should().Contain("The tonal range of the instrument");
    }

    [Test]
    public void Frozen_status_disables_the_slider()
    {
        // act
        var component = RenderTonalRange(ConfigurationStatus.DisabledAndLocked);

        // assert
        component.FindAll("input.mud-slider-input").Should().AllSatisfy(input => input.HasAttribute("disabled").Should().BeTrue());
    }

    private IRenderedComponent<InstrumentTonalRange> RenderTonalRange(ConfigurationStatus status = ConfigurationStatus.Enabled) => _testContext.RenderComponent<InstrumentTonalRange>(parameters => parameters
        .Add(component => component.Instrument, Instrument.One)
        .Add(component => component.Status, status)
    );
}
