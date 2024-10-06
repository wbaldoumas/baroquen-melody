using BaroquenMelody.Library;
using BaroquenMelody.Library.Midi;
using CommunityToolkit.Maui.Storage;
using System.Globalization;
using MauiFileSystem = Microsoft.Maui.Storage.FileSystem;

namespace BaroquenMelody.App.Infrastructure.FileSystem;

internal sealed class MauiMidiSaver : IMidiSaver
{
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

        var stream = File.OpenRead(tempPath);

        await using (stream.ConfigureAwait(false))
        {
            var fileSaverResult = await FileSaver.Default.SaveAsync("Baroquen Melody.mid", stream, cancellationToken).ConfigureAwait(false);

            return fileSaverResult.IsSuccessful;
        }
    }
}
