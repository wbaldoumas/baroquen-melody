using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Effects;

public sealed class CompositionConfigurationEffects(IState<InstrumentConfigurationState> instrumentConfigurationState)
{
    [EffectMethod]
    public async Task HandleUpdateCompositionConfiguration(UpdateCompositionConfiguration action, IDispatcher dispatcher)
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
                    instrumentConfiguration.MidiProgram,
                    instrumentConfiguration.IsEnabled,
                    IsUserApplied: false
                )
            );
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }
}
