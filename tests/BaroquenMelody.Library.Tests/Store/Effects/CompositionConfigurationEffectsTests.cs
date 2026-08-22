using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Configurations.Enums;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Effects;
using BaroquenMelody.Library.Store.Reducers;
using BaroquenMelody.Library.Store.State;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Standards;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Effects;

[TestFixture]
internal sealed class CompositionConfigurationEffectsTests
{
    private IDispatcher _mockDispatcher = null!;

    private IState<InstrumentConfigurationState> _mockInstrumentConfigurationState = null!;

    private CompositionConfigurationEffects _compositionConfigurationEffects = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockInstrumentConfigurationState = Substitute.For<IState<InstrumentConfigurationState>>();

        _compositionConfigurationEffects = new CompositionConfigurationEffects(
            _mockInstrumentConfigurationState
        );
    }

    [Test]
    public async Task HandleUpdateCompositionConfiguration_updates_instrument_configurations_as_expected()
    {
        // arrange
        var lastUserAppliedConfigurations = new Dictionary<Instrument, InstrumentConfiguration>
        {
            {
                Instrument.One,
                new InstrumentConfiguration(
                    Instrument.One,
                    Notes.C4,
                    Notes.C5,
                    InstrumentConfiguration.DefaultMinVelocity,
                    InstrumentConfiguration.DefaultMaxVelocity,
                    GeneralMidi2Program.AcousticGrandPiano,
                    ConfigurationStatus.Enabled
                )
            },
            {
                Instrument.Two,
                new InstrumentConfiguration(
                    Instrument.Two,
                    Notes.G3,
                    Notes.B4,
                    InstrumentConfiguration.DefaultMinVelocity,
                    InstrumentConfiguration.DefaultMaxVelocity,
                    GeneralMidi2Program.AcousticGrandPiano,
                    ConfigurationStatus.Enabled
                )
            },
            {
                Instrument.Three,
                new InstrumentConfiguration(
                    Instrument.Three,
                    Notes.D3,
                    Notes.F4,
                    InstrumentConfiguration.DefaultMinVelocity,
                    InstrumentConfiguration.DefaultMaxVelocity,
                    GeneralMidi2Program.AcousticGrandPiano,
                    ConfigurationStatus.Enabled
                )
            },
            {
                Instrument.Four,
                new InstrumentConfiguration(
                    Instrument.Four,
                    Notes.C2,
                    Notes.E3,
                    InstrumentConfiguration.DefaultMinVelocity,
                    InstrumentConfiguration.DefaultMaxVelocity,
                    GeneralMidi2Program.AcousticGrandPiano,
                    ConfigurationStatus.Enabled
                )
            }
        };

        _mockInstrumentConfigurationState.Value.Returns(
            new InstrumentConfigurationState(
                new Dictionary<Instrument, InstrumentConfiguration>(),
                lastUserAppliedConfigurations
            )
        );

        var action = new UpdateCompositionConfiguration(NoteName.D, Mode.Ionian, Meter.FourFour);

        // act
        await _compositionConfigurationEffects.HandleUpdateCompositionConfigurationAsync(action, _mockDispatcher);

        // assert
        _mockDispatcher.Received().Dispatch(
            Arg.Is<UpdateInstrumentConfiguration>(
                updateInstrumentConfiguration => updateInstrumentConfiguration.Instrument == Instrument.One &&
                                                 updateInstrumentConfiguration.MinNote == Notes.B3 &&
                                                 updateInstrumentConfiguration.MaxNote == Notes.B4 &&
                                                 updateInstrumentConfiguration.Status == ConfigurationStatus.Enabled &&
                                                 !updateInstrumentConfiguration.IsUserApplied)
        );
    }

    [Test]
    public async Task HandleLoadSavedCompositionConfigurationAsync_dispatches_expected_actions()
    {
        // arrange
        var configuration = TestCompositionConfigurations.Get();

        var action = new LoadSavedCompositionConfiguration(configuration);

        // act
        await CompositionConfigurationEffects.HandleLoadSavedCompositionConfigurationAsync(action, _mockDispatcher);

        // assert
        _mockDispatcher.Received().Dispatch(
            Arg.Is<LoadCompositionConfiguration>(
                loadCompositionConfiguration => loadCompositionConfiguration.CompositionConfiguration == configuration
            )
        );

        foreach (var instrumentConfiguration in configuration.InstrumentConfigurations)
        {
            _mockDispatcher.Received().Dispatch(
                Arg.Is<UpdateInstrumentConfiguration>(
                    updateInstrumentConfiguration => updateInstrumentConfiguration.Instrument == instrumentConfiguration.Instrument &&
                                                     updateInstrumentConfiguration.MinNote == instrumentConfiguration.MinNote &&
                                                     updateInstrumentConfiguration.MaxNote == instrumentConfiguration.MaxNote &&
                                                     updateInstrumentConfiguration.MidiProgram == instrumentConfiguration.MidiProgram &&
                                                     updateInstrumentConfiguration.Status == instrumentConfiguration.Status &&
                                                     updateInstrumentConfiguration.IsUserApplied)
            );
        }

        foreach (var compositionRule in configuration.AggregateCompositionRuleConfiguration.Configurations)
        {
            _mockDispatcher.Received().Dispatch(
                Arg.Is<UpdateCompositionRuleConfiguration>(
                    updateCompositionRuleConfiguration => updateCompositionRuleConfiguration.CompositionRule == compositionRule.Rule &&
                                                          updateCompositionRuleConfiguration.Status == compositionRule.Status &&
                                                          updateCompositionRuleConfiguration.Strictness == compositionRule.Strictness
                )
            );
        }

        foreach (var ornamentation in configuration.AggregateOrnamentationConfiguration.Configurations)
        {
            _mockDispatcher.Received().Dispatch(
                Arg.Is<UpdateCompositionOrnamentationConfiguration>(
                    updateCompositionOrnamentationConfiguration => updateCompositionOrnamentationConfiguration.OrnamentationType == ornamentation.OrnamentationType &&
                                                                   updateCompositionOrnamentationConfiguration.Status == ornamentation.Status &&
                                                                   updateCompositionOrnamentationConfiguration.Probability == ornamentation.Probability
                )
            );
        }
    }

    [Test]
    public async Task HandleLoadSavedCompositionConfigurationAsync_leaves_the_default_for_ornamentations_the_saved_configuration_lacks()
    {
        // arrange - a save from before an ornamentation existed carries no entry for it; the newer
        // ornamentation must keep working on legacy saves, so the STATE that results from folding the
        // load's dispatches over the Defaults-initialized dictionary must still hold the default entry.
        // Asserting the folded state (not just the dispatcher) decides the mechanism: a rewrite to a
        // whole-set batch replacement would wipe the entry while dispatching nothing per-entry.
        var legacyOrnamentationConfigurations = AggregateOrnamentationConfiguration.Default.Configurations
            .Where(static ornamentationConfiguration => ornamentationConfiguration.OrnamentationType != OrnamentationType.Arpeggio)
            .ToHashSet();

        var configuration = TestCompositionConfigurations.Get() with
        {
            AggregateOrnamentationConfiguration = new AggregateOrnamentationConfiguration(legacyOrnamentationConfigurations)
        };

        var dispatchedActions = new List<object>();

        _mockDispatcher.When(static dispatcher => dispatcher.Dispatch(Arg.Any<object>()))
            .Do(callInfo => dispatchedActions.Add(callInfo.Arg<object>()));

        // act
        await CompositionConfigurationEffects.HandleLoadSavedCompositionConfigurationAsync(new LoadSavedCompositionConfiguration(configuration), _mockDispatcher);

        // assert - fold every dispatched ornamentation action through the real reducers over the default
        // state, exactly as the store would
        var foldedState = new CompositionOrnamentationConfigurationState();

        foreach (var dispatchedAction in dispatchedActions)
        {
            foldedState = dispatchedAction switch
            {
                UpdateCompositionOrnamentationConfiguration update => CompositionOrnamentationConfigurationReducers.ReduceUpdateCompositionOrnamentationConfiguration(foldedState, update),
                BatchUpdateCompositionOrnamentationConfiguration batch => CompositionOrnamentationConfigurationReducers.ReduceBatchUpdateCompositionOrnamentationConfiguration(foldedState, batch),
                _ => foldedState
            };
        }

        foldedState[OrnamentationType.Arpeggio].Should().Be(
            new OrnamentationConfiguration(OrnamentationType.Arpeggio, ConfigurationStatus.Enabled, 25),
            "the ornamentation the save predates must keep its default after the load");
    }
}
