using BaroquenMelody.App.Components.Shared;
using Bunit;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using MudBlazor;
using NUnit.Framework;

namespace BaroquenMelody.App.Components.Tests.Shared;

[TestFixture]
internal sealed class InstrumentRangePlayerTests
{
    private AppComponentsTestContext _testContext = null!;

    [SetUp]
    public void SetUp() => _testContext = new AppComponentsTestContext();

    [TearDown]
    public void TearDown() => _testContext.Dispose();

    [Test]
    public void Renders_distinct_example_midi_for_the_low_and_high_notes()
    {
        // act
        var component = RenderPlayer();

        // assert
        var lowPlayerSource = component.Find("#low-note-player").GetAttribute("src");
        var highPlayerSource = component.Find("#high-note-player").GetAttribute("src");

        lowPlayerSource.Should().StartWith("data:audio/midi;base64,").And.NotBe("data:audio/midi;base64,");
        highPlayerSource.Should().StartWith("data:audio/midi;base64,").And.NotBe("data:audio/midi;base64,");
        lowPlayerSource.Should().NotBe(highPlayerSource);
    }

    [Test]
    public void Playing_a_note_starts_the_midi_player()
    {
        // arrange
        var component = RenderPlayer();

        // act
        component.InvokeAsync(() => component.Instance.PlayLowNote()).GetAwaiter().GetResult();

        // assert
        _testContext.JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "startMidiPlayer");
    }

    [Test]
    public void Failure_to_start_the_midi_player_shows_an_error_toast()
    {
        // arrange
        _testContext.JSInterop.SetupVoid("startMidiPlayer", _ => true).SetException(new JSException("no midi driver"));

        var component = RenderPlayer();

        // act
        component.InvokeAsync(() => component.Instance.PlayHighNote()).GetAwaiter().GetResult();

        // assert
        _testContext.Services.GetRequiredService<ISnackbar>().ShownSnackbars.Should().ContainSingle();
    }

    private IRenderedComponent<InstrumentRangePlayer> RenderPlayer() => _testContext.RenderComponent<InstrumentRangePlayer>(parameters => parameters
        .Add(component => component.LowNote, Notes.C3)
        .Add(component => component.HighNote, Notes.C6)
        .Add(component => component.MidiInstrument, GeneralMidi2Program.AcousticGrandPiano)
    );
}
