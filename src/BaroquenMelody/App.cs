using BaroquenMelody.Infrastructure.State;
using BaroquenMelody.Library;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums.Extensions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody;

internal sealed class App : IDisposable
{
    private readonly IBaroquenMelodyComposerConfigurator _configurator;

    private readonly IDisposable _stateSubscription;

    private readonly IState<CompositionConfigurationState> _compositionConfigurationState;

    private readonly IState<InstrumentConfigurationState> _instrumentConfigurationState;

    private readonly IState<CompositionRuleConfigurationState> _compositionRuleConfigurationState;

    private readonly IState<CompositionOrnamentationConfigurationState> _compositionOrnamentationConfigurationState;

    public App(
        IStore store,
        IBaroquenMelodyComposerConfigurator configurator,
        IState<CompositionProgressState> compositionProgressState,
        IState<CompositionConfigurationState> compositionConfigurationState,
        IState<InstrumentConfigurationState> instrumentConfigurationState,
        IState<CompositionRuleConfigurationState> compositionRuleConfigurationState,
        IState<CompositionOrnamentationConfigurationState> compositionOrnamentationConfigurationState)
    {
        store.InitializeAsync().Wait();

        _configurator = configurator;
        _compositionConfigurationState = compositionConfigurationState;
        _instrumentConfigurationState = instrumentConfigurationState;
        _compositionRuleConfigurationState = compositionRuleConfigurationState;
        _compositionOrnamentationConfigurationState = compositionOrnamentationConfigurationState;

        _stateSubscription = compositionProgressState
            .ObserveChanges()
            .Subscribe(ReportCompositionProgress);
    }

    public Library.BaroquenMelody Run()
    {
        var compositionConfiguration = new CompositionConfiguration(
            _instrumentConfigurationState.Value.EnabledConfigurations,
            PhrasingConfiguration.Default,
            _compositionRuleConfigurationState.Value.Aggregate,
            _compositionOrnamentationConfigurationState.Value.Aggregate,
            _compositionConfigurationState.Value.TonicNote,
            _compositionConfigurationState.Value.Mode,
            _compositionConfigurationState.Value.Meter,
            _compositionConfigurationState.Value.Meter.DefaultMusicalTimeSpan(),
            _compositionConfigurationState.Value.MinimumMeasures,
            Tempo: _compositionConfigurationState.Value.Tempo
        );

        return _configurator.Configure(compositionConfiguration).Compose(CancellationToken.None);
    }

    public void Dispose() => _stateSubscription.Dispose();

    private static void ReportCompositionProgress(IState<CompositionProgressState> state) => Console.WriteLine($"Current composition step: {state.Value.CurrentStep}\n");
}
