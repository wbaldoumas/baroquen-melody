using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Infrastructure.Extensions;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class OrnamentationConfigurationCardTests
{
    private AppComponentsTestContext _testContext = null!;

    private OrnamentationType _ornamentationType;

    [SetUp]
    public void SetUp()
    {
        _testContext = new AppComponentsTestContext();
        _ornamentationType = _testContext.Services.GetRequiredService<IOrnamentationConfigurationService>().ConfigurableOrnamentations.First();
    }

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Card_renders_the_ornamentation_name_and_its_example_player()
    {
        // act
        var component = RenderCard();

        // assert
        component.Markup.Should().Contain(_ornamentationType.ToSpaceSeparatedString());
        component.Find("#ornamentation-player").GetAttribute("src").Should().StartWith("data:audio/midi;base64,").And.NotBe("data:audio/midi;base64,");
    }

    [Test]
    public void Disabling_the_ornamentation_updates_the_store()
    {
        // arrange
        var component = RenderCard();

        // act
        component.EnableSwitch().Change(false);

        // assert
        _testContext.StateOf<CompositionOrnamentationConfigurationState>()[_ornamentationType]!.Status.Should().Be(ConfigurationStatus.Disabled);
    }

    [Test]
    public void Changing_the_probability_updates_the_store_and_keeps_the_status()
    {
        // arrange
        var component = RenderCard();

        // act
        component.Find("input[type=number]").Change("42");

        // assert
        var configuration = _testContext.StateOf<CompositionOrnamentationConfigurationState>()[_ornamentationType]!;

        configuration.Probability.Should().Be(42);
        configuration.Status.Should().Be(ConfigurationStatus.Enabled);
    }

    [Test]
    public void Clicking_play_starts_the_example_midi_player()
    {
        // arrange
        var component = RenderCard();

        // act: the play icon is the first button in the card header
        component.FindAll("button")[0].Click();

        // assert
        _testContext.JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "startMidiPlayer");
    }

    [Test]
    public void Failure_to_play_the_example_shows_an_error_toast()
    {
        // arrange
        _testContext.JSInterop.SetupVoid("startMidiPlayer", _ => true).SetException(new Microsoft.JSInterop.JSException("no midi driver"));

        var component = RenderCard();

        // act
        component.FindAll("button")[0].Click();

        // assert
        _testContext.Services.GetRequiredService<MudBlazor.ISnackbar>().ShownSnackbars.Should().ContainSingle();
    }

    private IRenderedComponent<OrnamentationConfigurationCard> RenderCard() => _testContext.RenderComponent<OrnamentationConfigurationCard>(parameters => parameters
        .Add(component => component.OrnamentationType, _ornamentationType)
    );
}
