using BaroquenMelody.Library.Compositions.Midi;
using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;
using IDispatcher = Fluxor.IDispatcher;

namespace BaroquenMelody.App.Infrastructure.FileSystem;

internal sealed class MauiMidiLauncher(IState<BaroquenMelodyState> state, IDispatcher dispatcher, IMidiSaver midiSaver) : IMidiLauncher
{
    private const string Title = "Open MIDI File";

    public async Task LaunchAsync(string path, CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            path = await midiSaver.SaveTempAsync(state.Value.BaroquenMelody!, cancellationToken).ConfigureAwait(false);

            dispatcher.Dispatch(new UpdateBaroquenMelody(state.Value.BaroquenMelody!, path, state.Value.HasBeenSaved));
        }

        await Launcher.Default.OpenAsync(new OpenFileRequest(Title, new ReadOnlyFile(path))).ConfigureAwait(false);
    }
}
