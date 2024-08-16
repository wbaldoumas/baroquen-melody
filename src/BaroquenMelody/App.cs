using BaroquenMelody.Library;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Configurations.Services;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Infrastructure.State;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody;

internal sealed class App : IDisposable
{
    private readonly IBaroquenMelodyComposerConfigurator _configurator;

    private readonly IDisposable _stateSubscription;

    private readonly IState<CompositionConfigurationState> _compositionConfigurationState;

    private readonly IState<VoiceConfigurationState> _voiceConfigurationState;

    private readonly IState<CompositionRuleConfigurationState> _compositionRuleConfigurationState;

    private readonly IState<CompositionOrnamentationConfigurationState> _compositionOrnamentationConfigurationState;

    public App(
        IStore store,
        IBaroquenMelodyComposerConfigurator configurator,
        ICompositionConfigurationService compositionConfigurationService,
        IState<CompositionProgressState> compositionProgressState,
        IState<CompositionConfigurationState> compositionConfigurationState,
        IState<VoiceConfigurationState> voiceConfigurationState,
        IState<CompositionRuleConfigurationState> compositionRuleConfigurationState,
        IState<CompositionOrnamentationConfigurationState> compositionOrnamentationConfigurationState)
    {
        store.InitializeAsync().Wait();
        compositionConfigurationService.ConfigureDefaults();

        _configurator = configurator;
        _compositionConfigurationState = compositionConfigurationState;
        _voiceConfigurationState = voiceConfigurationState;
        _compositionRuleConfigurationState = compositionRuleConfigurationState;
        _compositionOrnamentationConfigurationState = compositionOrnamentationConfigurationState;

        _stateSubscription = compositionProgressState
            .ObserveChanges()
            .Subscribe(ReportCompositionProgress);
    }

    public Library.BaroquenMelody Run()
    {
        var compositionConfiguration = new CompositionConfiguration(
            _voiceConfigurationState.Value.Aggregate,
            PhrasingConfiguration.Default,
            _compositionRuleConfigurationState.Value.Aggregate,
            _compositionOrnamentationConfigurationState.Value.Aggregate,
            BaroquenScale.Parse($"{_compositionConfigurationState.Value.RootNote} {_compositionConfigurationState.Value.Mode}"),
            _compositionConfigurationState.Value.Meter,
            _compositionConfigurationState.Value.Meter.DefaultMusicalTimeSpan(),
            _compositionConfigurationState.Value.CompositionLength
        );

        return _configurator.Configure(compositionConfiguration).Compose();
    }

    public void Dispose() => _stateSubscription.Dispose();

    private static void ReportCompositionProgress(IState<CompositionProgressState> state) => Console.WriteLine($"Current composition step: {state.Value.CurrentStep}\n");
}
