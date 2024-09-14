using BaroquenMelody.Library.Infrastructure.FileSystem;

namespace BaroquenMelody.App.Infrastructure.FileSystem;

internal sealed class MauiDeviceDirectoryProvider : IDeviceDirectoryProvider
{
    public string AppDataDirectory => Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory;
}
