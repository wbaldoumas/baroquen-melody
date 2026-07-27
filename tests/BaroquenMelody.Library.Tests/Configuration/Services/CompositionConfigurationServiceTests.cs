using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configuration.Services;

[TestFixture]
internal sealed class CompositionConfigurationServiceTests
{
    private IDispatcher _mockDispatcher = null!;

    private IState<CompositionConfigurationState> _mockCompositionConfigurationState = null!;

    private CompositionConfigurationService _compositionConfigurationService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockCompositionConfigurationState = Substitute.For<IState<CompositionConfigurationState>>();

        _mockCompositionConfigurationState.Value.Returns(new CompositionConfigurationState());

        _compositionConfigurationService = new CompositionConfigurationService(_mockDispatcher, _mockCompositionConfigurationState);
    }

    [Test]
    public void ConfigurableRootNotes_returns_expected_values()
    {
        // arrange
        var expectedConfigurableRootNotes = new[]
        {
            NoteName.C,
            NoteName.CSharp,
            NoteName.D,
            NoteName.DSharp,
            NoteName.E,
            NoteName.F,
            NoteName.FSharp,
            NoteName.G,
            NoteName.GSharp,
            NoteName.A,
            NoteName.ASharp,
            NoteName.B
        };

        // act
        var actualConfigurableRootNotes = _compositionConfigurationService.ConfigurableRootNotes;

        // assert
        actualConfigurableRootNotes.Should().BeEquivalentTo(expectedConfigurableRootNotes);
    }

    [Test]
    public void ConfigurableScaleModes_returns_expected_values()
    {
        // arrange
        var expectedConfigurableScaleModes = new[]
        {
            Mode.Ionian,
            Mode.Dorian,
            Mode.Phrygian,
            Mode.Lydian,
            Mode.Mixolydian,
            Mode.Aeolian,
            Mode.Locrian
        };

        // act
        var actualConfigurableScaleModes = _compositionConfigurationService.ConfigurableScaleModes;

        // assert
        actualConfigurableScaleModes.Should().BeEquivalentTo(expectedConfigurableScaleModes);
    }

    [Test]
    public void ConfigurableMeters_returns_expected_values()
    {
        // arrange
        var expectedConfigurableMeters = new[]
        {
            Meter.FourFour,
            Meter.ThreeFour,
            Meter.FiveEight
        };

        // act
        var actualConfigurableMeters = _compositionConfigurationService.ConfigurableMeters;

        // assert
        actualConfigurableMeters.Should().BeEquivalentTo(expectedConfigurableMeters);
    }

    [Test]
    public void ConfigurableCompositionForms_returns_expected_values()
    {
        // arrange
        var expectedConfigurableCompositionForms = new[]
        {
            CompositionForm.Fugue,
            CompositionForm.GroundBass
        };

        // act
        var actualConfigurableCompositionForms = _compositionConfigurationService.ConfigurableCompositionForms;

        // assert
        actualConfigurableCompositionForms.Should().BeEquivalentTo(expectedConfigurableCompositionForms);
    }

    [Test]
    public void Randomize_dispatches_expected_update()
    {
        // act
        _compositionConfigurationService.Randomize();

        // assert
        _mockDispatcher.Received(1).Dispatch(Arg.Any<UpdateCompositionConfiguration>());
    }

    [Test]
    public void Randomize_preserves_the_selected_composition_form()
    {
        // arrange: the form is a deliberate structural choice, so rolling the dice must not change it.
        _mockCompositionConfigurationState.Value.Returns(new CompositionConfigurationState { Form = CompositionForm.GroundBass });

        // act
        _compositionConfigurationService.Randomize();

        // assert
        _mockDispatcher.Received(1).Dispatch(Arg.Is<UpdateCompositionConfiguration>(static action => action.Form == CompositionForm.GroundBass));
    }

    [Test]
    public void Reset_dispatches_expected_update()
    {
        // act
        _compositionConfigurationService.Reset();

        // assert
        _mockDispatcher.Received(1).Dispatch(Arg.Is<UpdateCompositionConfiguration>(static action => action.Form == CompositionForm.Fugue));
    }
}
