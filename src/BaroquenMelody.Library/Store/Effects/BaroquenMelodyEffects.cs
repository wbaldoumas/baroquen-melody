using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums.Extensions;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Effects;

public sealed class BaroquenMelodyEffects(
    IState<CompositionConfigurationState> compositionConfigurationState,
    IState<InstrumentConfigurationState> instrumentConfigurationState,
    IState<CompositionRuleConfigurationState> compositionRuleConfigurationState,
    IState<CompositionOrnamentationConfigurationState> compositionOrnamentationConfigurationState,
    IBaroquenMelodyComposerConfigurator baroquenMelodyComposerConfigurator
)
{
    [EffectMethod]
    public async Task HandleCompose(Compose action, IDispatcher dispatcher)
    {
        var compositionConfiguration = new CompositionConfiguration(
            instrumentConfigurationState.Value.EnabledConfigurations,
            PhrasingConfiguration.Default,
            compositionRuleConfigurationState.Value.Aggregate,
            compositionOrnamentationConfigurationState.Value.Aggregate,
            compositionConfigurationState.Value.Scale,
            compositionConfigurationState.Value.Meter,
            compositionConfigurationState.Value.Meter.DefaultMusicalTimeSpan(),
            compositionConfigurationState.Value.MinimumMeasures
        );

        var baroquenMelody = await Task.Run(
            () => baroquenMelodyComposerConfigurator.Configure(compositionConfiguration).Compose()
        ).ConfigureAwait(false);

        dispatcher.Dispatch(new UpdateBaroquenMelody(baroquenMelody));
    }
}
