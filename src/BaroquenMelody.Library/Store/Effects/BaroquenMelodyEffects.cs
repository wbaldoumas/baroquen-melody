using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Enums.Extensions;
using BaroquenMelody.Library.Midi;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Effects;

public sealed class BaroquenMelodyEffects(
    IState<CompositionConfigurationState> compositionConfigurationState,
    IState<InstrumentConfigurationState> instrumentConfigurationState,
    IState<CompositionRuleConfigurationState> compositionRuleConfigurationState,
    IState<CompositionOrnamentationConfigurationState> compositionOrnamentationConfigurationState,
    IMidiSaver midiSaver,
    IBaroquenMelodyComposerConfigurator baroquenMelodyComposerConfigurator
) : IDisposable
{
    private CancellationTokenSource? _cancellationTokenSource;

    [EffectMethod]
    public async Task HandleCompose(Compose action, IDispatcher dispatcher)
    {
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();

        var compositionConfiguration = new CompositionConfiguration(
            instrumentConfigurationState.Value.EnabledConfigurations,
            PhrasingConfiguration.Default,
            compositionRuleConfigurationState.Value.Aggregate,
            compositionOrnamentationConfigurationState.Value.Aggregate,
            compositionConfigurationState.Value.TonicNote,
            compositionConfigurationState.Value.Mode,
            compositionConfigurationState.Value.Meter,
            compositionConfigurationState.Value.Meter.DefaultMusicalTimeSpan(),
            compositionConfigurationState.Value.MinimumMeasures,
            Tempo: compositionConfigurationState.Value.Tempo
        );

        await Task.Run(
            async () =>
            {
                try
                {
                    var baroquenMelody = baroquenMelodyComposerConfigurator.Configure(compositionConfiguration).Compose(_cancellationTokenSource.Token);
                    var path = await midiSaver.SaveTempAsync(baroquenMelody, _cancellationTokenSource.Token).ConfigureAwait(false);

                    dispatcher.Dispatch(new UpdateBaroquenMelody(baroquenMelody, path, HasBeenSaved: false));
                }
                catch (OperationCanceledException)
                {
                    dispatcher.Dispatch(new ResetCompositionProgress());
                }
                catch (Exception)
                {
                    dispatcher.Dispatch(new MarkCompositionFailed());
                }
                finally
                {
                    _cancellationTokenSource?.Dispose();
                }
            },
            _cancellationTokenSource.Token
        ).ConfigureAwait(false);
    }

    [EffectMethod]
    public async Task HandleCancelComposition(CancelComposition action, IDispatcher dispatcher)
    {
        var cancellationRequestTask = _cancellationTokenSource?.CancelAsync();

        if (cancellationRequestTask is not null)
        {
            await cancellationRequestTask.ConfigureAwait(false);
        }
    }

    public void Dispose() => _cancellationTokenSource?.Dispose();
}
