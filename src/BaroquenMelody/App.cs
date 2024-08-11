using BaroquenMelody.Library;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody;

public class App(IStore store, IBaroquenMelodyComposerConfigurator configurator, IState<CompositionProgressState> compositionProgressState)
{
    public async Task<Library.BaroquenMelody> RunAsync(CompositionConfiguration compositionConfiguration)
    {
        await store.InitializeAsync().ConfigureAwait(false);

        compositionProgressState.StateChanged += (_, _) => { Console.WriteLine($"Current composition step: {compositionProgressState.Value.CurrentStep}\n"); };

        return configurator.Configure(compositionConfiguration).Compose();
    }
}
