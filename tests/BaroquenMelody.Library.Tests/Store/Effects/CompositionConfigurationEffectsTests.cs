using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory.Enums;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Effects;
using BaroquenMelody.Library.Store.State;
using Fluxor;
using Melanchall.DryWetMidi.MusicTheory;
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
                    Notes.C5
                )
            },
            {
                Instrument.Two,
                new InstrumentConfiguration(
                    Instrument.Two,
                    Notes.G3,
                    Notes.B4
                )
            },
            {
                Instrument.Three,
                new InstrumentConfiguration(
                    Instrument.Three,
                    Notes.D3,
                    Notes.F4
                )
            },
            {
                Instrument.Four,
                new InstrumentConfiguration(
                    Instrument.Four,
                    Notes.C2,
                    Notes.E3
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
        await _compositionConfigurationEffects.HandleUpdateCompositionConfiguration(action, _mockDispatcher);

        // assert
        _mockDispatcher.Received().Dispatch(
            Arg.Is<UpdateInstrumentConfiguration>(
                updateInstrumentConfiguration => updateInstrumentConfiguration.Instrument == Instrument.One &&
                                                 updateInstrumentConfiguration.MinNote == Notes.B3 &&
                                                 updateInstrumentConfiguration.MaxNote == Notes.B4 &&
                                                 updateInstrumentConfiguration.IsEnabled == true &&
                                                 updateInstrumentConfiguration.IsUserApplied == false
            )
        );
    }
}
