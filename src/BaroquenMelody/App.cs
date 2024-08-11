using BaroquenMelody.Library;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Infrastructure.State;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody;

internal sealed class App : IDisposable
{
    private readonly IBaroquenMelodyComposerConfigurator _configurator;

    private readonly IDisposable _stateSubscription;

    public App(IStore store, IBaroquenMelodyComposerConfigurator configurator, IState<CompositionProgressState> compositionProgressState)
    {
        store.InitializeAsync().Wait();

        _configurator = configurator;

        _stateSubscription = compositionProgressState
            .ObserveChanges()
            .Subscribe(ReportCompositionProgress);
    }

    public Library.BaroquenMelody Run(CompositionConfiguration compositionConfiguration) => _configurator.Configure(compositionConfiguration).Compose();

    public void Dispose() => _stateSubscription.Dispose();

    private static void ReportCompositionProgress(IState<CompositionProgressState> state) => Console.WriteLine($"Current composition step: {state.Value.CurrentStep}\n");
}
