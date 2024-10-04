using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Effects;

public sealed class CompositionConfigurationEffects(IState<InstrumentConfigurationState> instrumentConfigurationState)
{
    [EffectMethod]
    public async Task HandleUpdateCompositionConfigurationAsync(UpdateCompositionConfiguration action, IDispatcher dispatcher)
    {
        var scale = new BaroquenScale(action.RootNote, action.Mode);

        foreach (var instrumentConfiguration in instrumentConfigurationState.Value.LastUserAppliedConfigurations.Values)
        {
            var closestMinNote = scale.GetNotes()
                .OrderBy(note => Math.Abs(note.NoteNumber - instrumentConfiguration.MinNote.NoteNumber))
                .First();

            var closestMaxNote = scale.GetNotes()
                .OrderBy(note => Math.Abs(note.NoteNumber - instrumentConfiguration.MaxNote.NoteNumber))
                .First();

            dispatcher.Dispatch(
                new UpdateInstrumentConfiguration(
                    instrumentConfiguration.Instrument,
                    closestMinNote,
                    closestMaxNote,
                    instrumentConfiguration.MinVelocity,
                    instrumentConfiguration.MaxVelocity,
                    instrumentConfiguration.MidiProgram,
                    instrumentConfiguration.Status,
                    IsUserApplied: false
                )
            );
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    [EffectMethod]
    public static async Task HandleLoadSavedCompositionConfigurationAsync(LoadSavedCompositionConfiguration action, IDispatcher dispatcher)
    {
        var configuration = action.CompositionConfiguration;

        dispatcher.Dispatch(new LoadCompositionConfiguration(configuration));

        foreach (var instrumentConfiguration in configuration.InstrumentConfigurations)
        {
            dispatcher.Dispatch(
                new UpdateInstrumentConfiguration(
                    instrumentConfiguration.Instrument,
                    instrumentConfiguration.MinNote,
                    instrumentConfiguration.MaxNote,
                    instrumentConfiguration.MinVelocity,
                    instrumentConfiguration.MaxVelocity,
                    instrumentConfiguration.MidiProgram,
                    instrumentConfiguration.Status,
                    IsUserApplied: true
                )
            );
        }

        foreach (var compositionRule in configuration.AggregateCompositionRuleConfiguration.Configurations)
        {
            dispatcher.Dispatch(new UpdateCompositionRuleConfiguration(
                compositionRule.Rule,
                compositionRule.Status,
                compositionRule.Strictness
            ));
        }

        foreach (var ornamentation in configuration.AggregateOrnamentationConfiguration.Configurations)
        {
            dispatcher.Dispatch(new UpdateCompositionOrnamentationConfiguration(
                ornamentation.OrnamentationType,
                ornamentation.Status,
                ornamentation.Probability
            ));
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
