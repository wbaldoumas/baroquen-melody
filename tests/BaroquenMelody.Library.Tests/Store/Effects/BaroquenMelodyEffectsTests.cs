using BaroquenMelody.Library.Compositions.Composers;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Infrastructure.FileSystem;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.Effects;
using BaroquenMelody.Library.Store.State;
using Fluxor;
using Melanchall.DryWetMidi.Core;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Store.Effects;

[TestFixture]
internal sealed class BaroquenMelodyEffectsTests
{
    private IState<CompositionConfigurationState> _mockCompositionConfigurationState = null!;

    private IState<InstrumentConfigurationState> _mockInstrumentConfigurationState = null!;

    private IState<CompositionRuleConfigurationState> _mockCompositionRuleConfigurationState = null!;

    private IState<CompositionOrnamentationConfigurationState> _mockCompositionOrnamentationConfigurationState = null!;

    private IBaroquenMelodyComposerConfigurator _mockBaroquenMelodyComposerConfigurator = null!;

    private IBaroquenMelodyComposer _mockComposer = null!;

    private IDispatcher _mockDispatcher = null!;

    private IMidiSaver _mockMidiSaver = null!;

    private BaroquenMelodyEffects _baroquenMelodyEffects = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionConfigurationState = Substitute.For<IState<CompositionConfigurationState>>();
        _mockInstrumentConfigurationState = Substitute.For<IState<InstrumentConfigurationState>>();
        _mockCompositionRuleConfigurationState = Substitute.For<IState<CompositionRuleConfigurationState>>();
        _mockCompositionOrnamentationConfigurationState = Substitute.For<IState<CompositionOrnamentationConfigurationState>>();
        _mockBaroquenMelodyComposerConfigurator = Substitute.For<IBaroquenMelodyComposerConfigurator>();
        _mockComposer = Substitute.For<IBaroquenMelodyComposer>();
        _mockDispatcher = Substitute.For<IDispatcher>();
        _mockMidiSaver = Substitute.For<IMidiSaver>();

        _baroquenMelodyEffects = new BaroquenMelodyEffects(
            _mockCompositionConfigurationState,
            _mockInstrumentConfigurationState,
            _mockCompositionRuleConfigurationState,
            _mockCompositionOrnamentationConfigurationState,
            _mockMidiSaver,
            _mockBaroquenMelodyComposerConfigurator
        );
    }

    [Test]
    public async Task HandleCompose_DispatchesUpdateBaroquenMelody()
    {
        // arrange
        _mockInstrumentConfigurationState.Value.Returns(new InstrumentConfigurationState());
        _mockCompositionRuleConfigurationState.Value.Returns(new CompositionRuleConfigurationState());
        _mockCompositionOrnamentationConfigurationState.Value.Returns(new CompositionOrnamentationConfigurationState());
        _mockCompositionConfigurationState.Value.Returns(new CompositionConfigurationState());
        _mockBaroquenMelodyComposerConfigurator.Configure(Arg.Any<CompositionConfiguration>()).Returns(_mockComposer);

        var baroquenMelody = new BaroquenMelody(new MidiFile());

        _mockComposer.Compose(Arg.Any<CancellationToken>()).Returns(baroquenMelody);

        // act
        await _baroquenMelodyEffects.HandleCompose(new Compose(), _mockDispatcher);

        // assert
        _mockDispatcher.Received().Dispatch(Arg.Is<UpdateBaroquenMelody>(action => action.BaroquenMelody == baroquenMelody));
    }

    [Test]
    public async Task HandleCancelCompose_CancelsCompose()
    {
        // arrange
        _mockInstrumentConfigurationState.Value.Returns(new InstrumentConfigurationState());
        _mockCompositionRuleConfigurationState.Value.Returns(new CompositionRuleConfigurationState());
        _mockCompositionOrnamentationConfigurationState.Value.Returns(new CompositionOrnamentationConfigurationState());
        _mockCompositionConfigurationState.Value.Returns(new CompositionConfigurationState());
        _mockBaroquenMelodyComposerConfigurator.Configure(Arg.Any<CompositionConfiguration>()).Returns(_mockComposer);

        _mockComposer.Compose(Arg.Any<CancellationToken>()).Throws<OperationCanceledException>();

        // act
        await _baroquenMelodyEffects.HandleCompose(new Compose(), _mockDispatcher);

        // assert
        _mockDispatcher.Received().Dispatch(Arg.Any<ResetCompositionProgress>());
    }

    [Test]
    public async Task HandleCancelComposition_gracefully_handles_cancellation()
    {
        // act
        await _baroquenMelodyEffects.HandleCancelComposition(new CancelComposition(), _mockDispatcher);

        // assert
        _mockDispatcher.DidNotReceiveWithAnyArgs().Dispatch(Arg.Any<object>());
    }

    [TearDown]
    public void TearDown()
    {
        _baroquenMelodyEffects.Dispose();
    }
}
