using BaroquenMelody.App.Components.Pages;
using BaroquenMelody.App.Components.Tests.TestComponents;
using BaroquenMelody.Library;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Melanchall.DryWetMidi.Core;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Pages;

[TestFixture]
internal sealed class HomeTests
{
    private AppComponentsTestContext _testContext = null!;

    private IRenderedComponent<MudDialogProvider> _dialogProvider = null!;

    private ISnackbar Snackbar => _testContext.Services.GetRequiredService<ISnackbar>();

    [SetUp]
    public void SetUp()
    {
        var mockComposer = Substitute.For<IMidiFileComposer>();

        mockComposer.Compose(Arg.Any<CancellationToken>()).Returns(_ => new MidiFileComposition(new MidiFile(new TrackChunk())));

        var mockComposerConfigurator = Substitute.For<IBaroquenMelodyComposerConfigurator>();

        mockComposerConfigurator.Configure(Arg.Any<CompositionConfiguration>()).Returns(mockComposer);

        _testContext = new AppComponentsTestContext(services => services.AddSingleton(mockComposerConfigurator));

        _testContext.MockMidiSaver.SaveTempAsync(Arg.Any<MidiFileComposition>(), Arg.Any<CancellationToken>()).Returns("temp/path.mid");
        _testContext.MockMidiSaver.SaveAsync(Arg.Any<MidiFileComposition>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        _dialogProvider = _testContext.RenderComponent<MudDialogProvider>();
    }

    [TearDown]
    public void TearDown()
    {
        _dialogProvider.Dispose();
        _testContext.Dispose();
    }

    [Test]
    public void Home_renders_the_configuration_tabs()
    {
        // act
        var component = _testContext.RenderComponent<Home>();

        // assert
        component.Markup.Should().ContainAll("General", "Instrumentation", "Rules", "Ornamentation", "Composition");
    }

    [Test]
    public void Compose_button_is_disabled_when_the_instrumentation_is_invalid()
    {
        // arrange: instrument four is disabled by default
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentConfigurationStatus(Instrument.One, Library.Configurations.Enums.ConfigurationStatus.Disabled));
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentConfigurationStatus(Instrument.Two, Library.Configurations.Enums.ConfigurationStatus.Disabled));
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentConfigurationStatus(Instrument.Three, Library.Configurations.Enums.ConfigurationStatus.Disabled));

        // act
        var component = _testContext.RenderComponent<Home>();

        // assert
        ComposeButton(component).HasAttribute("disabled").Should().BeTrue();
    }

    [Test]
    public void Clicking_compose_produces_a_composition_and_activates_the_composition_tab()
    {
        // arrange
        var component = _testContext.RenderComponent<Home>();

        // act
        ComposeButton(component).Click();

        // assert
        component.WaitForAssertion(() =>
        {
            _testContext.StateOf<BaroquenMelodyState>().Path.Should().Be("temp/path.mid");

            var activeTab = component.Find(".mud-tab-active");

            activeTab.TextContent.Should().Contain("Composition");
        });
    }

    [Test]
    public void Composing_over_an_unsaved_composition_asks_for_confirmation()
    {
        // arrange
        _testContext.Dispatcher.Dispatch(new UpdateBaroquenMelody(new MidiFileComposition(new MidiFile(new TrackChunk())), "existing/path.mid", HasBeenSaved: false));

        var component = _testContext.RenderComponent<Home>();

        // act
        ComposeButton(component).Click();

        // assert
        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Previous composition has not yet been saved"));
    }

    [Test]
    public async Task Accepting_the_save_confirmation_saves_before_composing()
    {
        // arrange
        _testContext.Dispatcher.Dispatch(new UpdateBaroquenMelody(new MidiFileComposition(new MidiFile(new TrackChunk())), "existing/path.mid", HasBeenSaved: false));

        var component = _testContext.RenderComponent<Home>();

        ComposeButton(component).Click();
        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Previous composition"));

        // act
        _dialogProvider.FindAll("button").Single(button => button.TextContent.Trim() == "Yes").Click();

        // assert
        component.WaitForAssertion(() => _testContext.StateOf<BaroquenMelodyState>().Path.Should().Be("temp/path.mid"));
        await _testContext.MockMidiSaver.Received(1).SaveAsync(Arg.Any<MidiFileComposition>(), "existing/path.mid", Arg.Any<CancellationToken>());
    }

    [Test]
    public void Save_configuration_button_opens_the_save_dialog()
    {
        // arrange
        var component = _testContext.RenderComponent<Home>();

        // act
        component.FindAll("button").Single(button => button.TextContent.Contains("Save Configuration", StringComparison.Ordinal)).Click();

        // assert
        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Save Composition Configuration"));
    }

    [Test]
    public void Failed_composition_toasts_when_not_on_the_composition_tab()
    {
        // arrange
        _testContext.RenderComponent<Home>();

        // act
        _testContext.Dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Failed));

        // assert
        Snackbar.ShownSnackbars.Should().ContainSingle(snackbar => snackbar.Message != null && snackbar.Message.Contains("Failed to compose", StringComparison.Ordinal));
    }

    [Test]
    public void Completed_composition_toasts_when_not_on_the_composition_tab()
    {
        // arrange
        _testContext.RenderComponent<Home>();

        // act
        _testContext.Dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Complete));

        // assert
        Snackbar.ShownSnackbars.Should().ContainSingle(snackbar => snackbar.Message != null && snackbar.Message.Contains("Composition complete", StringComparison.Ordinal));
    }

    [Test]
    public void Unmounted_home_page_no_longer_toasts()
    {
        // arrange: mount and unmount the page, as navigating away does
        var component = _testContext.RenderComponent<ConditionalWrapper>(parameters => parameters
            .Add(wrapper => wrapper.Show, true)
            .AddChildContent<Home>()
        );

        component.SetParametersAndRender(parameters => parameters.Add(wrapper => wrapper.Show, false));

        // act
        _testContext.Dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Failed));

        // assert: a leaked progress subscription from the unmounted page would still toast
        Snackbar.ShownSnackbars.Should().BeEmpty();
    }

    private static AngleSharp.Dom.IElement ComposeButton(IRenderedComponent<Home> component) => component
        .FindAll("button")
        .Single(button => button.TextContent.Trim() == "Compose");
}
