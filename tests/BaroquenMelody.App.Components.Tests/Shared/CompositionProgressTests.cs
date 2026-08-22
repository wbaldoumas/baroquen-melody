using BaroquenMelody.App.Components.Shared;
using BaroquenMelody.Library;
using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Bunit;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.Core;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class CompositionProgressTests
{
    private AppComponentsTestContext _testContext = null!;

    private IBaroquenMelodyComposerConfigurator _mockComposerConfigurator = null!;

    private IRenderedComponent<MudDialogProvider> _dialogProvider = null!;

    private ISnackbar Snackbar => _testContext.Services.GetRequiredService<ISnackbar>();

    [SetUp]
    public void SetUp()
    {
        var mockComposer = Substitute.For<IMidiFileComposer>();

        mockComposer.Compose(Arg.Any<CancellationToken>()).Returns(_ => new MidiFileComposition(new MidiFile(new TrackChunk())));

        _mockComposerConfigurator = Substitute.For<IBaroquenMelodyComposerConfigurator>();
        _mockComposerConfigurator.Configure(Arg.Any<CompositionConfiguration>()).Returns(mockComposer);

        _testContext = new AppComponentsTestContext(services => services.AddSingleton(_mockComposerConfigurator));

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
    public void Waiting_state_renders_an_enabled_compose_button()
    {
        // act
        var component = _testContext.RenderComponent<CompositionProgress>();

        // assert
        var composeButton = component.Find("button.compose-button");

        composeButton.TextContent.Should().Contain("Compose");
        composeButton.HasAttribute("disabled").Should().BeFalse();
    }

    [Test]
    public void Compose_button_is_disabled_when_the_instrumentation_is_invalid()
    {
        // arrange: instrument four is disabled by default
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentConfigurationStatus(Instrument.One, ConfigurationStatus.Disabled));
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentConfigurationStatus(Instrument.Two, ConfigurationStatus.Disabled));
        _testContext.Dispatcher.Dispatch(new UpdateInstrumentConfigurationStatus(Instrument.Three, ConfigurationStatus.Disabled));

        // act
        var component = _testContext.RenderComponent<CompositionProgress>();

        // assert
        component.Find("button.compose-button").HasAttribute("disabled").Should().BeTrue();
    }

    [Test]
    public void Clicking_compose_produces_a_new_composition()
    {
        // arrange
        var component = _testContext.RenderComponent<CompositionProgress>();

        // act
        component.Find("button.compose-button").Click();

        // assert
        component.WaitForAssertion(() => _testContext.StateOf<BaroquenMelodyState>().Path.Should().Be("temp/path.mid"));
    }

    [Test]
    public void Loading_state_shows_the_progress_message_and_percentage()
    {
        // arrange
        _testContext.Dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Theme));
        _testContext.Dispatcher.Dispatch(new ProgressCompositionThemeProgress(60));

        // act
        var component = _testContext.RenderComponent<CompositionProgress>();

        // assert: 60% of the theme step is 20% overall
        component.Markup.Should().Contain("Composing main theme...");
        component.Markup.Should().Contain("20%");
    }

    [Test]
    public void Cancel_button_dispatches_a_cancellation()
    {
        // arrange
        var wasCancellationDispatched = false;

        _testContext.Services.GetRequiredService<IActionSubscriber>().SubscribeToAction<CancelComposition>(this, _ => wasCancellationDispatched = true);
        _testContext.Dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Theme));

        var component = _testContext.RenderComponent<CompositionProgress>();

        // act
        component.FindAll("button").Single(button => button.TextContent.Contains("Cancel Composition", StringComparison.Ordinal)).Click();

        // assert
        wasCancellationDispatched.Should().BeTrue();
    }

    [Test]
    public void Starting_a_composition_stops_the_midi_player()
    {
        // arrange: the component subscribes after its first render
        var component = _testContext.RenderComponent<CompositionProgress>();

        // act
        _testContext.Dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Theme));

        // assert
        component.WaitForAssertion(() => _testContext.JSInterop.Invocations
            .Should().Contain(invocation => invocation.Identifier == "stopMidiPlayer"));
    }

    [Test]
    public void Failed_state_shows_an_alert_that_resets_on_close()
    {
        // arrange
        _testContext.Dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Failed));

        var component = _testContext.RenderComponent<CompositionProgress>();

        component.Markup.Should().Contain("Failed to compose");

        // act
        component.Find(".mud-alert button").Click();

        // assert
        _testContext.StateOf<CompositionProgressState>().IsWaiting.Should().BeTrue();
    }

    [Test]
    public void Completed_composition_renders_the_midi_player()
    {
        // arrange
        DispatchCompletedComposition();

        // act
        var component = _testContext.RenderComponent<CompositionProgress>();

        // assert
        component.Find("#player").GetAttribute("src").Should().StartWith("data:audio/midi;base64,");
        component.Find("#visualizer").GetAttribute("src").Should().StartWith("data:audio/midi;base64,");
    }

    [Test]
    public void Saving_the_composition_toasts_and_marks_it_saved()
    {
        // arrange
        DispatchCompletedComposition();

        var component = _testContext.RenderComponent<CompositionProgress>();

        // act
        component.FindAll("button").Single(button => button.TextContent.Contains("Save Composition", StringComparison.Ordinal)).Click();

        // assert
        component.WaitForAssertion(() =>
        {
            _testContext.StateOf<BaroquenMelodyState>().HasBeenSaved.Should().BeTrue();
            Snackbar.ShownSnackbars.Should().ContainSingle();
        });
    }

    [Test]
    public void Failing_to_save_the_composition_does_not_mark_it_saved()
    {
        // arrange
        _testContext.MockMidiSaver.SaveAsync(Arg.Any<MidiFileComposition>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        DispatchCompletedComposition();

        var component = _testContext.RenderComponent<CompositionProgress>();

        // act
        component.FindAll("button").Single(button => button.TextContent.Contains("Save Composition", StringComparison.Ordinal)).Click();

        // assert: a failed save leaves the composition unsaved and shows no confirmation snackbar
        component.WaitForAssertion(() =>
        {
            _testContext.StateOf<BaroquenMelodyState>().HasBeenSaved.Should().BeFalse();
            Snackbar.ShownSnackbars.Should().BeEmpty();
        });
    }

    [Test]
    public void Composing_over_an_unsaved_composition_asks_for_confirmation()
    {
        // arrange: an unsaved composition exists while waiting to compose
        _testContext.Dispatcher.Dispatch(new UpdateBaroquenMelody(new MidiFileComposition(new MidiFile(new TrackChunk())), "existing/path.mid", HasBeenSaved: false));

        var component = _testContext.RenderComponent<CompositionProgress>();

        // act
        component.Find("button.compose-button").Click();

        // assert
        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Previous composition has not yet been saved"));
    }

    [Test]
    public async Task Declining_the_save_confirmation_composes_without_saving()
    {
        // arrange
        _testContext.Dispatcher.Dispatch(new UpdateBaroquenMelody(new MidiFileComposition(new MidiFile(new TrackChunk())), "existing/path.mid", HasBeenSaved: false));

        var component = _testContext.RenderComponent<CompositionProgress>();

        component.Find("button.compose-button").Click();
        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Previous composition"));

        // act
        _dialogProvider.FindAll("button").Single(button => button.TextContent.Contains("No", StringComparison.Ordinal)).Click();

        // assert: a new composition replaces the old path without saving the old one
        component.WaitForAssertion(() => _testContext.StateOf<BaroquenMelodyState>().Path.Should().Be("temp/path.mid"));
        await _testContext.MockMidiSaver.DidNotReceive().SaveAsync(Arg.Any<MidiFileComposition>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Cancelling_the_save_confirmation_aborts_composing()
    {
        // arrange
        _testContext.Dispatcher.Dispatch(new UpdateBaroquenMelody(new MidiFileComposition(new MidiFile(new TrackChunk())), "existing/path.mid", HasBeenSaved: false));

        var component = _testContext.RenderComponent<CompositionProgress>();

        component.Find("button.compose-button").Click();
        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Previous composition"));

        // act
        _dialogProvider.FindAll("button").Single(button => button.TextContent.Trim() == "Cancel").Click();

        // assert: the dialog closes and no new composition replaces the existing one
        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().NotContain("Previous composition"));
        _testContext.StateOf<BaroquenMelodyState>().Path.Should().Be("existing/path.mid");
        await _testContext.MockMidiSaver.DidNotReceive().SaveTempAsync(Arg.Any<MidiFileComposition>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Failing_to_save_aborts_composing()
    {
        // arrange
        _testContext.MockMidiSaver.SaveAsync(Arg.Any<MidiFileComposition>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _testContext.Dispatcher.Dispatch(new UpdateBaroquenMelody(new MidiFileComposition(new MidiFile(new TrackChunk())), "existing/path.mid", HasBeenSaved: false));

        var component = _testContext.RenderComponent<CompositionProgress>();

        component.Find("button.compose-button").Click();
        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Previous composition"));

        // act
        _dialogProvider.FindAll("button").Single(button => button.TextContent.Trim() == "Yes").Click();

        // assert: the failed save keeps the old composition and no new one is composed
        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().NotContain("Previous composition"));
        _testContext.StateOf<BaroquenMelodyState>().Path.Should().Be("existing/path.mid");
        _testContext.StateOf<BaroquenMelodyState>().HasBeenSaved.Should().BeFalse();
        await _testContext.MockMidiSaver.DidNotReceive().SaveTempAsync(Arg.Any<MidiFileComposition>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Accepting_the_save_confirmation_saves_before_composing()
    {
        // arrange
        _testContext.Dispatcher.Dispatch(new UpdateBaroquenMelody(new MidiFileComposition(new MidiFile(new TrackChunk())), "existing/path.mid", HasBeenSaved: false));

        var component = _testContext.RenderComponent<CompositionProgress>();

        component.Find("button.compose-button").Click();
        _dialogProvider.WaitForAssertion(() => _dialogProvider.Markup.Should().Contain("Previous composition"));

        // act
        _dialogProvider.FindAll("button").Single(button => button.TextContent.Contains("Yes", StringComparison.Ordinal)).Click();

        // assert: the old composition is saved, then a new one is composed
        component.WaitForAssertion(() =>
        {
            _testContext.StateOf<BaroquenMelodyState>().Path.Should().Be("temp/path.mid");
        });

        await _testContext.MockMidiSaver.Received(1).SaveAsync(Arg.Any<MidiFileComposition>(), "existing/path.mid", Arg.Any<CancellationToken>());
    }

    private void DispatchCompletedComposition()
    {
        _testContext.Dispatcher.Dispatch(new UpdateBaroquenMelody(new MidiFileComposition(new MidiFile(new TrackChunk())), "temp/path.mid", HasBeenSaved: false));
        _testContext.Dispatcher.Dispatch(new ProgressCompositionStep(CompositionStep.Complete));
    }
}
