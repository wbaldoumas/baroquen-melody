using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Melanchall.DryWetMidi.Common;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class InstrumentVelocityRangeTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Slider_labels_show_the_stored_velocity_range()
    {
        // act: instruments default to velocities 50 through 60
        var component = RenderVelocityRange();

        // assert
        component.Markup.Should().ContainAll("50%", "60%");
    }

    [Test]
    public void Sliding_the_lower_thumb_updates_the_stored_minimum_velocity()
    {
        // arrange
        var component = RenderVelocityRange();

        // act: the upper slider input renders first, the lower second
        component.FindAll("input.mud-slider-input")[1].Input("30");

        // assert
        var configuration = _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!;

        configuration.MinVelocity.Should().Be(new SevenBitNumber(30));
        configuration.MaxVelocity.Should().Be(new SevenBitNumber(60));
    }

    [Test]
    public void Sliding_the_upper_thumb_updates_the_stored_maximum_velocity()
    {
        // arrange
        var component = RenderVelocityRange();

        // act
        component.FindAll("input.mud-slider-input")[0].Input("90");

        // assert
        var configuration = _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!;

        configuration.MinVelocity.Should().Be(new SevenBitNumber(50));
        configuration.MaxVelocity.Should().Be(new SevenBitNumber(90));
    }

    [Test]
    public void Raising_the_minimum_above_the_maximum_pulls_the_maximum_along()
    {
        // arrange
        var component = RenderVelocityRange();

        // act: 80 exceeds the default maximum of 60
        component.FindAll("input.mud-slider-input")[1].Input("80");

        // assert
        var configuration = _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!;

        configuration.MinVelocity.Should().Be(new SevenBitNumber(80));
        configuration.MaxVelocity.Should().Be(new SevenBitNumber(80));
    }

    [Test]
    public void Lowering_the_maximum_below_the_minimum_pushes_the_minimum_down()
    {
        // arrange
        var component = RenderVelocityRange();

        // act: 30 is below the default minimum of 50
        component.FindAll("input.mud-slider-input")[0].Input("30");

        // assert
        var configuration = _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!;

        configuration.MinVelocity.Should().Be(new SevenBitNumber(30));
        configuration.MaxVelocity.Should().Be(new SevenBitNumber(30));
    }

    [Test]
    public void External_state_changes_are_reflected_in_the_rendered_range()
    {
        // arrange
        var component = RenderVelocityRange();

        // act
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentVelocities(Instrument.One, new SevenBitNumber(20), new SevenBitNumber(95)));
        component.Render();

        // assert
        component.Markup.Should().ContainAll("20%", "95%");
    }

    [Test]
    public void Muted_and_quiet_velocities_render_their_own_icons()
    {
        // arrange
        var component = RenderVelocityRange();

        // act: zero renders the muted icon and anything below fifty the quiet icon
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentVelocities(Instrument.One, new SevenBitNumber(0), new SevenBitNumber(30)));
        component.Render();

        // assert
        component.Markup.Should().ContainAll("0%", "30%");
    }

    [Test]
    public void Clicking_the_help_icon_opens_the_popover()
    {
        // arrange
        var component = RenderVelocityRange();

        // act
        component.Find("button").Click();

        // assert
        _testContext.PopoverProvider.Markup.Should().Contain("dynamic range");
    }

    [Test]
    public void Frozen_status_disables_the_slider()
    {
        // act
        var component = RenderVelocityRange(ConfigurationStatus.DisabledAndLocked);

        // assert
        component.FindAll("input.mud-slider-input").Should().AllSatisfy(input => input.HasAttribute("disabled").Should().BeTrue());
    }

    private IRenderedComponent<InstrumentVelocityRange> RenderVelocityRange(ConfigurationStatus status = ConfigurationStatus.Enabled) => _testContext.RenderComponent<InstrumentVelocityRange>(parameters => parameters
        .Add(component => component.Instrument, Instrument.One)
        .Add(component => component.Status, status)
    );
}
