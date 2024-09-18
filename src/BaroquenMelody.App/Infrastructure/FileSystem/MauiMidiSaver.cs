using BaroquenMelody.Library.Infrastructure.FileSystem;
using CommunityToolkit.Maui.Storage;
using System.Globalization;
using MauiFileSystem = Microsoft.Maui.Storage.FileSystem;

namespace BaroquenMelody.App.Infrastructure.FileSystem;

internal sealed class MauiMidiSaver : IMidiSaver
{
    public Task<string> SaveTempAsync(Library.BaroquenMelody baroquenMelody, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<string>(cancellationToken);
        }

        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
        var path = Path.Combine(MauiFileSystem.CacheDirectory, $"Baroquen Melody ({timestamp}).mid");

        baroquenMelody.MidiFile.Write(path);

        return Task.FromResult(path);
    }

    public async Task<bool> SaveAsync(Library.BaroquenMelody baroquenMelody, string tempPath, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return false;
        }

        if (!File.Exists(tempPath))
        {
            tempPath = await SaveTempAsync(baroquenMelody, cancellationToken).ConfigureAwait(false);
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
