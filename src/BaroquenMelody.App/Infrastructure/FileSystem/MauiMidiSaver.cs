using BaroquenMelody.Library;
using BaroquenMelody.Library.Midi;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using System.Globalization;
using MauiFileSystem = Microsoft.Maui.Storage.FileSystem;

namespace BaroquenMelody.App.Infrastructure.FileSystem;

internal sealed class MauiMidiSaver : IMidiSaver
{
#if WINDOWS
    private readonly IWindowsSaveFileDialog _saveFileDialog;

    public MauiMidiSaver(IWindowsSaveFileDialog saveFileDialog) => _saveFileDialog = saveFileDialog;
#endif

    public Task<string> SaveTempAsync(MidiFileComposition midiFileComposition, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<string>(cancellationToken);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
        var path = Path.Combine(MauiFileSystem.CacheDirectory, $"baroquen-melody-{timestamp}.mid");

        midiFileComposition.MidiFile.Write(path);

        return Task.FromResult(path);
    }

    public async Task<bool> SaveAsync(MidiFileComposition midiFileComposition, string tempPath, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        if (!File.Exists(tempPath))
        {
            tempPath = await SaveTempAsync(midiFileComposition, cancellationToken).ConfigureAwait(false);
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        return await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var isSuccessful = await SavePlatformAsync(tempPath, cancellationToken).ConfigureAwait(true);

            await Toast.Make(
                isSuccessful ? "MIDI file saved successfully." : "Failed to save the MIDI file.",
                isSuccessful ? ToastDuration.Short : ToastDuration.Long).Show(cancellationToken).ConfigureAwait(true);

            return isSuccessful;
        }).ConfigureAwait(false);
    }

#if WINDOWS
    // The WinRT FileSavePicker (used directly and by CommunityToolkit's FileSaver) throws
    // E_FAIL (0x80004005) in unpackaged and elevated desktop processes, so on Windows we
    // present the classic Win32 save dialog and copy the temp file to the chosen location.
    private async Task<bool> SavePlatformAsync(string tempPath, CancellationToken cancellationToken)
    {
        var destinationPath = _saveFileDialog.Show("Baroquen Melody.mid");

        if (string.IsNullOrEmpty(destinationPath))
        {
            return false; // user cancelled
        }

        var sourceStream = File.OpenRead(tempPath);

        await using (sourceStream.ConfigureAwait(true))
        {
            var destinationStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

            await using (destinationStream.ConfigureAwait(true))
            {
                await sourceStream.CopyToAsync(destinationStream, cancellationToken).ConfigureAwait(true);
            }
        }

        return true;
    }
#else
    private static async Task<bool> SavePlatformAsync(string tempPath, CancellationToken cancellationToken)
    {
        var stream = File.OpenRead(tempPath);

        await using (stream.ConfigureAwait(false))
        {
            var fileSaverResult = await FileSaver.Default
                .SaveAsync("Baroquen Melody.mid", stream, cancellationToken)
                .ConfigureAwait(false);

            return fileSaverResult.IsSuccessful;
        }
    }
#endif
}
