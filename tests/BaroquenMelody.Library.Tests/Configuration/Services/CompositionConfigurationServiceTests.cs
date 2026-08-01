using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Forms;
using BaroquenMelody.Library.Forms.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Configuration.Services;

[TestFixture]
internal sealed class CompositionConfigurationServiceTests
{
    private IDispatcher _mockDispatcher = null!;

    private IState<CompositionConfigurationState> _mockCompositionConfigurationState = null!;

    private IState<InstrumentConfigurationState> _mockInstrumentConfigurationState = null!;

    private GroundBassFeasibilityAnalyzer _groundBassFeasibilityAnalyzer = null!;

    private CompositionConfigurationService _compositionConfigurationService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockCompositionConfigurationState = Substitute.For<IState<CompositionConfigurationState>>();
        _mockInstrumentConfigurationState = Substitute.For<IState<InstrumentConfigurationState>>();
        _groundBassFeasibilityAnalyzer = new GroundBassFeasibilityAnalyzer();

        _mockCompositionConfigurationState.Value.Returns(new CompositionConfigurationState());
        _mockInstrumentConfigurationState.Value.Returns(new InstrumentConfigurationState());

        _compositionConfigurationService = new CompositionConfigurationService(
            _mockDispatcher,
            _mockCompositionConfigurationState,
            _mockInstrumentConfigurationState,
            _groundBassFeasibilityAnalyzer
        );
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
    public void ConfigurableGroundBassPatterns_returns_the_free_draw_then_the_bank_in_order()
    {
        // arrange
        var expectedConfigurableGroundBassPatterns = new GroundBass?[]
        {
            null,
            GroundBass.DescendingTetrachord,
            GroundBass.Romanesca,
            GroundBass.CadentialGround
        };

        // act
        var actualConfigurableGroundBassPatterns = _compositionConfigurationService.ConfigurableGroundBassPatterns;

        // assert: the free draw leads so it renders first in the dropdown, then the bank in bank order.
        actualConfigurableGroundBassPatterns.Should().Equal(expectedConfigurableGroundBassPatterns);
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
    public void Randomize_with_the_ground_bass_form_draws_a_pattern_that_fits_the_randomized_key()
    {
        // arrange: a G3-B4 bass hosts only the tetrachord in some keys and the whole bank in others, so a
        // valid roll must consult feasibility against the key rolled into the same action - pinning an
        // infeasible pattern would silently turn the randomized composition into a fugue.
        _mockCompositionConfigurationState.Value.Returns(new CompositionConfigurationState { Form = CompositionForm.GroundBass });
        _mockInstrumentConfigurationState.Value.Returns(new InstrumentConfigurationState(BuildTenorBassConfigurations(), BuildTenorBassConfigurations()));

        var dispatchedActions = new List<UpdateCompositionConfiguration>();

        _mockDispatcher.When(static dispatcher => dispatcher.Dispatch(Arg.Any<UpdateCompositionConfiguration>()))
            .Do(callInfo => dispatchedActions.Add(callInfo.Arg<UpdateCompositionConfiguration>()));

        // act
        for (var roll = 0; roll < 50; ++roll)
        {
            _compositionConfigurationService.Randomize();
        }

        // assert: every roll keeps the form and lands on the free draw or a pattern feasible for that
        // roll's own key; the distinctness check is an existence property, not a per-roll pin.
        foreach (var dispatchedAction in dispatchedActions)
        {
            dispatchedAction.Form.Should().Be(CompositionForm.GroundBass);

            if (dispatchedAction.GroundBassPattern is { } pattern)
            {
                _groundBassFeasibilityAnalyzer.GetFeasibleGroundBasses(
                    _mockInstrumentConfigurationState.Value.EnabledConfigurations,
                    new BaroquenScale(dispatchedAction.RootNote, dispatchedAction.Mode)
                ).Should().Contain(pattern, "the randomized pattern must fit the randomized key {0} {1}", dispatchedAction.RootNote, dispatchedAction.Mode);
            }
        }

        dispatchedActions.Select(static dispatchedAction => dispatchedAction.GroundBassPattern).Distinct().Should().HaveCountGreaterThan(1, "fifty rolls should not all land on one pattern");
    }

    [Test]
    public void Randomize_with_the_fugue_form_preserves_the_configured_pattern()
    {
        // arrange: the pattern is inert under the fugue, so rolling the dice must not discard the user's
        // choice for their next ground bass composition.
        _mockCompositionConfigurationState.Value.Returns(new CompositionConfigurationState { Form = CompositionForm.Fugue, GroundBassPattern = GroundBass.Romanesca });

        // act
        _compositionConfigurationService.Randomize();

        // assert
        _mockDispatcher.Received(1).Dispatch(Arg.Is<UpdateCompositionConfiguration>(static action => action.GroundBassPattern == GroundBass.Romanesca));
    }

    [Test]
    public void Randomize_preserves_the_modulation_toggle()
    {
        // arrange: whether the piece journeys is a structural preference like the form itself, so the dice
        // must leave a disabled journey disabled - under either form.
        _mockCompositionConfigurationState.Value.Returns(new CompositionConfigurationState { Form = CompositionForm.GroundBass, GroundBassModulate = false });

        // act
        _compositionConfigurationService.Randomize();

        // assert
        _mockDispatcher.Received(1).Dispatch(Arg.Is<UpdateCompositionConfiguration>(static action => !action.GroundBassModulate));
    }

    [Test]
    public void Reset_dispatches_expected_update()
    {
        // arrange: a reset must also clear a pinned pattern back to the composer's free draw and restore
        // the default journey.
        _mockCompositionConfigurationState.Value.Returns(new CompositionConfigurationState { Form = CompositionForm.GroundBass, GroundBassPattern = GroundBass.CadentialGround, GroundBassModulate = false });

        // act
        _compositionConfigurationService.Reset();

        // assert
        _mockDispatcher.Received(1).Dispatch(Arg.Is<UpdateCompositionConfiguration>(static action => action.Form == CompositionForm.Fugue && action.GroundBassPattern == null && action.GroundBassModulate));
    }

    private static Dictionary<Instrument, InstrumentConfiguration> BuildTenorBassConfigurations() => new()
    {
        [Instrument.One] = new InstrumentConfiguration(Instrument.One, Notes.C5, Notes.E6, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
        [Instrument.Two] = new InstrumentConfiguration(Instrument.Two, Notes.E4, Notes.G5, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled),
        [Instrument.Three] = new InstrumentConfiguration(Instrument.Three, Notes.G3, Notes.B4, InstrumentConfiguration.DefaultMinVelocity, InstrumentConfiguration.DefaultMaxVelocity, GeneralMidi2Program.AcousticGrandPiano, ConfigurationStatus.Enabled)
    };
}
