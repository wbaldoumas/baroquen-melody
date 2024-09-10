using BaroquenMelody.Library.Infrastructure.FileSystem;

namespace BaroquenMelody.Infrastructure.FileSystem;

internal sealed class StubMidiLauncher : IMidiLauncher
{
    public Task LaunchAsync(string path, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
