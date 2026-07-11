using AngleSharp.Html.Dom;
using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class InstrumentConfigurationCardTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Card_renders_the_instrument_name_and_its_configuration_components()
    {
        // act
        var component = RenderCard(Instrument.One);

        // assert
        component.Markup.Should().Contain("Instrument One");
        component.FindComponents<MidiInstrumentAutocompleteSelector>().Should().ContainSingle();
        component.FindComponents<InstrumentTonalRange>().Should().ContainSingle();
        component.FindComponents<InstrumentVelocityRange>().Should().ContainSingle();
    }

    [TestCase(Instrument.One, true)]
    [TestCase(Instrument.Four, false)]
    public void Enable_switch_reflects_the_stored_instrument_status(Instrument instrument, bool expectedIsChecked)
    {
        // act
        var component = RenderCard(instrument);

        // assert
        var enableSwitch = (IHtmlInputElement)component.FindAll("input.mud-switch-input")[0];

        enableSwitch.IsChecked.Should().Be(expectedIsChecked);
    }

    [Test]
    public void Disabling_the_instrument_updates_the_store()
    {
        // arrange
        var component = RenderCard(Instrument.One);

        // act
        component.FindAll("input.mud-switch-input")[0].Change(false);

        // assert
        _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!.Status.Should().Be(ConfigurationStatus.Disabled);
    }

    [Test]
    public void Locking_the_instrument_updates_the_store()
    {
        // arrange
        var component = RenderCard(Instrument.One);

        // act
        component.FindAll("input.mud-switch-input")[1].Change(true);

        // assert
        _testContext.StateOf<InstrumentConfigurationState>()[Instrument.One]!.Status.Should().Be(ConfigurationStatus.EnabledAndLocked);
    }

    private IRenderedComponent<InstrumentConfigurationCard> RenderCard(Instrument instrument) => _testContext.RenderComponent<InstrumentConfigurationCard>(parameters => parameters
        .Add(component => component.Instrument, instrument)
    );
}
